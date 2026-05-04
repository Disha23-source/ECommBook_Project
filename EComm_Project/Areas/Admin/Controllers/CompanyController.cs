using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Models;
using EComm_Project_Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EComm_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int?id)
        {
            Company company = new Company();
            if(id == null) return View(company);
            company = _unitOfWork.Company.Get(id.GetValueOrDefault());
            if(company == null) return NotFound();
            return View(company);
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (company == null) return BadRequest();
            if (!ModelState.IsValid) return View(company);
            if (company.Id == 0)
                _unitOfWork.Company.Add(company);
            else
                _unitOfWork.Company.Update(company);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        #region APIs
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.Company.GetAll()});
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var companyInDB = _unitOfWork.Company.Get(id);
            if(companyInDB == null) 
                return Json(new {success= false,message="Unable to Delete Data!!!"});
            _unitOfWork.Company.Remove(companyInDB);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Data Delete Successfully!!!" });
        }
        #endregion
    }
}
