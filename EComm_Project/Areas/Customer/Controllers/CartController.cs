using EComm_Project_DataAccess.Repository;
using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Models;
using EComm_Project_Models.ViewModels;
using EComm_Project_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace EComm_Project.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private static bool isEmailConfirm = false;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SmsSender _smsSender;
        private readonly ITwilioServices _twilioServices;

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender,
            UserManager<IdentityUser> userManager, SmsSender smsSender, ITwilioServices twilioServices)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
            _smsSender = smsSender;
            _twilioServices = twilioServices;
        }


        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { set; get; }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //if (claims == null)
            //{
            //    ShoppingCartVM = new ShoppingCartVM()
            //    {
            //        ListCart = new List<ShoppingCart>()
            //    };
            //    return View(ShoppingCartVM);
            //}

            //To refresh the Cart Count after delete the product item from the Cart
            var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count();
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + "...";
                }
            }
            //Confirm Email
            if (!isEmailConfirm)
            {
                ViewBag.EmailMessage = "Email has been sent , Kindly verify your email!!!";
                ViewBag.EmailCSS = "text-success";
                isEmailConfirm = false;
            }
            else
            {
                ViewBag.EmailMessage = "Email must be confirm for authorize customer!!!";
                ViewBag.EmailCSS = "text-danger";
            }
            return View(ShoppingCartVM);
        }
        public IActionResult plus(int id)
        {
            var cart = _unitOfWork.ShoppingCart.Get(id);
            cart.Count += 1;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int id)
        {
            var cart = _unitOfWork.ShoppingCart.Get(id);
            if (cart.Count == 1)
                cart.Count = 1;
            else
                cart.Count -= 1;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult delete(int id)
        {

            var cart = _unitOfWork.ShoppingCart.Get(id);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        // SMS CODE-after placing order 
        private void SendOrderSms()
        {
            TempData["debug"] = "SMS method called";

            var phone = ShoppingCartVM.OrderHeader.PhoneNumber;

            if (!phone.StartsWith("+"))
            {
                phone = "+91" + phone;
            }

            try
            {
                _smsSender.SendOrderMessage(
                    phone,
                    ShoppingCartVM.OrderHeader.Name,
                    ShoppingCartVM.OrderHeader.Id,
                    ShoppingCartVM.OrderHeader.OrderTotal
                );

                TempData["success"] = "SMS sent successfully.";
            }
            catch (Exception ex)
            {
                TempData["error"] = "SMS failed: " + ex.Message;
            }

        }
        public IActionResult summary()
        {
            //Select the IDs we saved (Check checkbox)
            var idsString = TempData["SelectedCartIds"] as string;
            TempData.Keep("SelectedCartIds");
            if (string.IsNullOrEmpty(idsString))
                return RedirectToAction(nameof(Index));
            var selectedIds = idsString.Split(',').Select(int.Parse).ToList();

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                //Filter from DB to ONLY include selected items
                ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value && selectedIds.Contains(sc.Id), includeProperties: "Product"),

                //ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product"),
                OrderHeader = new OrderHeader(),

                //To show the multiple address in dropdownList
                UserAddresses = _unitOfWork.UserAddress.GetAll(u => u.ApplicationUserId == claims.Value)
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + "...";
                }
            }

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("summary")]
        public IActionResult summaryPost(string stripeToken,string paymentMethod)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // Fetch the IDs from TempData
            var idsString = TempData["SelectedCartIds"] as string;
            var selectedIds = new List<int>();
            if (!string.IsNullOrEmpty(idsString))
            {
                selectedIds = idsString.Split(',').Select(int.Parse).ToList();
            }

            //Filter from DB to ONLY include selected items
            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value && selectedIds.Contains(sc.Id),
                includeProperties: "Product").ToList();

            //To fetch all item from database
            //ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product");

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);

            ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price
                    , list.Product.Price50, list.Product.Price100);  
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                OrderDetail orderDetail = new OrderDetail()
                {
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    ProductId = list.ProductId,
                    Price = list.Price,
                    Count = list.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }
            _unitOfWork.Save();
            //Remove from Shopping Cart after order has placed 
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();

            // Session set
            var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);


            ////Update the Session Count of the cart to 0
            //HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, 0);

            //Stripe Payment
                if (stripeToken == null)
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                    ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                    _unitOfWork.Save();

                    SendOrderSms(); //SMS method called here 
                }
                else
                {
                    var options = new ChargeCreateOptions()
                    {
                        Amount = Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal * 100),
                        Currency = "usd",
                        Description = "OrderId:" + ShoppingCartVM.OrderHeader.Id.ToString(),
                        Source = stripeToken
                    };
                    var service = new ChargeService();
                    Charge charge = service.Create(options);
                    if (charge.BalanceTransactionId == null)
                        ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                    else
                        ShoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;
                    if (charge.Status.ToLower() == "succeeded")
                    {
                        ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                        ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                        ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
                        _unitOfWork.Save();

                        SendOrderSms(); //SMS method called here 
                    }
                }
            
           

            ////RazorPay Code-QR
            //if (paymentMethod == "Scanner")
            //{
            //    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            //    ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;

            //    _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            //    _unitOfWork.Save();

            //    return RedirectToAction(nameof (ScannerPayment), new { id = ShoppingCartVM.OrderHeader.Id });
                
            //}
            SendOrderSms(); //SMS method called here
            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }
        [HttpPost]
        public IActionResult PayPalCheckOut(string transactionId, string paymentResult)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            // Fetch the IDs from TempData
            var idsString = TempData["SelectedCartIds"] as string;
            var selectedIds = new List<int>();
            if (!string.IsNullOrEmpty(idsString))
            {
                selectedIds = idsString.Split(',').Select(int.Parse).ToList();
            }

            //Filter from DB to ONLY include selected items
            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value && selectedIds.Contains(sc.Id),
                includeProperties: "Product").ToList();

            //To fetch whole item from database
            //ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, includeProperties: "Product");

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);

            ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claims.Value;
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price
                    , list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Price * list.Count);
                OrderDetail orderDetail = new OrderDetail()
                {
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    ProductId = list.ProductId,
                    Price = list.Price,
                    Count = list.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
            }
            _unitOfWork.Save();


            //payment PayPal
            if (paymentResult != "Approved")
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusPending;
                _unitOfWork.Save();

                TempData["Error"] = "Payment was cancelled. Please try again!!!";
                return RedirectToAction(nameof(summary));
            }
            else
            {
                ShoppingCartVM.OrderHeader.TransactionId = transactionId;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.OrderStatusApproved;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            }
            _unitOfWork.Save();
            //Remove from Shopping Cart after order has placed 
            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();

            // Session set
            var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);

            SendOrderSms();  // SMS CODE - method called here
            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            //Code for sending proper image to the user
            var orderHeader = _unitOfWork.OrderHeader.FirstOrDefault(u => u.Id == id, includeProperties: "ApplicationUser");
            var orderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == id, includeProperties: "Product");

            var productNames = orderDetails.Select(u => u.Product.Title).ToList();
            var customerName = orderHeader.Name ?? "Customer";

            if (orderHeader != null)
            {
                string productTableRows = "";

                foreach (var item in orderDetails)
                {
                    productTableRows += $@"
                  <tr>
                    <td style='padding:10px; border-bottom:1px solid #eee;'>{item.Product.Title}</td>
                    <td style='padding:10px; border-bottom:1px solid #eee; text-align:center;'>{item.Count}</td>
                    <td style='padding:10px; border-bottom:1px solid #eee; text-align:right;'>{item.Price}</td>
                    <td style='padding:10px; border-bottom:1px solid #eee; text-align:right;'>{item.Price * item.Count}</td>
                  </tr>";
                }

                string emailHtml = $@"
                  <div style='font-family: sans-serif; max-width: 600px; border: 1px solid #ddd; padding: 20px;'>
                  <h2 style='color: #2c3e50;'>Order Confirmation</h2>
                  <p>Hi {orderHeader.Name}, your order <b>#{id}</b> is confirmed!</p>

                  <table style='width:100%; border-collapse: collapse;'>
                  <thead>
                  <tr style='background: #f8f9fa;'>
                   <th style='text-align:left; padding:10px;'>Product</th>
                   <th style='padding:10px;'>Qty</th>
                   <th style='text-align:right; padding:10px;'>Price</th>
                   <th style='text-align:right; padding:10px;'>Total</th>
                   </tr>
                   </thead>

                   <tbody>
                {productTableRows}
                </tbody>
                </table>

                <div style='text-align:right; margin-top:20px; font-size:1.2em;'>
                <b>Grand Total: ₹{orderHeader.OrderTotal}</b>
                 </div>

                 <p style='margin-top:30px; font-size:0.9em; color:#777;'>
                     Thank you for shopping with us!!! ❤</p>
                 </div>";
                try
                {
                    //Code for call to confirm the order
                    await _twilioServices.MakeOrderConfirmationCallAsync(orderHeader.PhoneNumber, id, customerName, productNames);
                    //call the function to send proper email to the user 
                    await _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, $"Order Confirmed #{id}" , emailHtml);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Twilio Notification Failed: {ex.Message}");
                }
            }
            return View(id);
        }


        // for checkbox applied Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost(int[] selectedItems)
        {
            //Code for email Comfirmation
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            if (user == null)
                ModelState.AddModelError(string.Empty, "Email Empty!!!");
            else if (!user.EmailConfirmed)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                return RedirectToAction(nameof(Index));

            }
            //Code to Select Product by checkbox in the cart
            if (selectedItems == null || selectedItems.Length == 0)
            {
                // No items selected
                return RedirectToAction(nameof(Index));
            }

            // We store the selected IDs in TempData so the Summary page can read them
            TempData["SelectedCartIds"] = string.Join(",", selectedItems);

            return RedirectToAction(nameof(summary));
        }

        //public IActionResult ScannerPayment(int id)
        //{
        //    var order = _unitOfWork.OrderHeader.FirstOrDefault(u => u.Id == id);

        //    if (order == null)
        //        return NotFound();

        //    return View(order);
        //}
    }
}

