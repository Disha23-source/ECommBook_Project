using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Models;
using EComm_Project_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EComm_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Upsert(int?id)
        {
            CoverType coverType = new CoverType();
            if (id == null) return View(coverType);
            coverType = _unitOfWork.CoverType.Get(id.GetValueOrDefault());
            if(coverType == null) return NotFound();
            return View(coverType);
        }
        [HttpPost]  
        public IActionResult Upsert(CoverType coverType)
        {
            if (coverType == null) return NotFound();
            if (!ModelState.IsValid) return View(coverType);
            if (coverType.Id == 0)
            _unitOfWork.CoverType.Add(coverType);
            else
                _unitOfWork.CoverType.Update(coverType);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Index()
        {
            return View();
        }
        #region API
        [HttpGet]
        public IActionResult GetAll() 
        {
            return Json(new {data= _unitOfWork.CoverType.GetAll()});
        }
        [HttpDelete]
        public IActionResult Delete(int id) 
        {
            var coverTypeInDb = _unitOfWork.CoverType.Get(id);
            if(coverTypeInDb == null) 
                return Json(new {success=false,message="Unable to Delete Data!!!"});
            _unitOfWork.CoverType.Remove(id);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Data Deleted Successfully!!!" });
        }
        #endregion
    }
}
