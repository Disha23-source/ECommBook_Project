using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EComm_Project.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class AddressController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public AddressController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(UserAddress userAddress)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claims == null)
            {
                return NotFound();
            }

            userAddress.ApplicationUserId = claims.Value;

            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("ApplicationUser");

            if (ModelState.IsValid)
            {
                _unitOfWork.UserAddress.Add(userAddress);
                _unitOfWork.Save();

                TempData["success"] = "Address added successfully";
                return RedirectToAction("summary", "Cart", new { area = "Customer" });
            }

            return View(userAddress);
        }
    }
}
