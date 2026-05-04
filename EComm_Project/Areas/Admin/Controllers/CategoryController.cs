using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Models;
using EComm_Project_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EComm_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int?id)
        {
            Category category = new Category();
            if(id == null) return View(category); //save
            //Edit 
            category = _unitOfWork.Category.Get(id.GetValueOrDefault());
            if(category == null) return NotFound();
            return View(category);
        }
        [HttpPost]
        public IActionResult Upsert(Category category)
        {
            if(category == null) return BadRequest();
            if(!ModelState.IsValid) return View (category);
            if (category.Id == 0)
                _unitOfWork.Category.Add(category);
            else
                _unitOfWork.Category.Update(category);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var categoryList = _unitOfWork.Category.GetAll();
            return Json(new {data=categoryList});
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var categoryInDb = _unitOfWork.Category.Get(id);//Find 
            if(categoryInDb == null)
                return Json(new { success = false ,message = "Unable to Delete Data!!!"});
            _unitOfWork.Category.Remove(categoryInDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Data Deleted Successfully!!!" });
            //return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
