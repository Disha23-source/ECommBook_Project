using EComm_Project_DataAccess.Repository;
using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Models;
using EComm_Project_Models.Models;
using EComm_Project_Models.ViewModels;
using EComm_Project_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace EComm_Project.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claims != null)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }

            var productList = _unitOfWork.Product.GetAll();
            return View(productList);
        }
   
        public IActionResult Details(int id)
        {
            // when user login through add to cart then see the no.of count of items 
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }

            var productInDb = _unitOfWork.Product.FirstOrDefault(p => p.Id == id , includeProperties:"Category,CoverType");
            if (productInDb == null) return NotFound();
            var shoppingCart = new ShoppingCart()
            {
                Product = productInDb,
                ProductId = id
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)(User.Identity);
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                if (claims == null) return NotFound();
                shoppingCart.ApplicationUserId = claims.Value;
                var shoppingCartInDb = _unitOfWork.ShoppingCart
                    .FirstOrDefault(sc => sc.ApplicationUserId == claims.Value 
                    && sc.ProductId == shoppingCart.ProductId);
                if (shoppingCartInDb == null)
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                else
                    shoppingCartInDb.Count += shoppingCart.Count;
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productInDb = _unitOfWork.Product.FirstOrDefault(p => p.Id == shoppingCart.Id, includeProperties: "Category,CoverType");
                if (productInDb == null) return NotFound();
                var shoppingCartEdit = new ShoppingCart()
                {
                    Product = productInDb,
                    ProductId = shoppingCart.Id
                };
                return View(shoppingCartEdit);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        //For search bar and category dropdown list in nav bar
        // Live search endpoint
        [HttpGet]
        public IActionResult SearchSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var results = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            p.Author.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(6)
                .Select(p => new { p.Id, p.Title, p.Author, p.ImageUrl })
                .ToList();

            return Json(results);
        }

        // Full search results page
        [HttpGet]
        public IActionResult Search(string query)
        {
            var results = _unitOfWork.Product.GetAll(includeProperties: "Category")
                .Where(p => p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            p.Author.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            ViewBag.Query = query;
            return View(results);
        }

        // Categories for dropdown
        [HttpGet]
        public IActionResult GetCategories()
        {
            var categories = _unitOfWork.Category.GetAll()
                .Select(c => new { c.Id, c.Name })
                .ToList();
            return Json(categories);
        }
    }
}
