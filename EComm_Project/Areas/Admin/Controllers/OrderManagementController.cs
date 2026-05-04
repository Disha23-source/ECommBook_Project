using EComm_Project_DataAccess.Repository.IRepository;
using EComm_Project_Models;
using EComm_Project_Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EComm_Project.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderManagementController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderManagementController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        // GET: /Order/AllOrders/GetAll  ← called by DataTables AJAX
        [HttpGet]
        public IActionResult GetAll()
        {
            IEnumerable<OrderHeader> orders = _unitOfWork.OrderHeader
                .GetAll(includeProperties: "ApplicationUser");

            return Json(new { data = orders });
        }

        // Keep AllOrders() just to return the View (no model needed now)
        public IActionResult AllOrders()
        {
            return View();
        }
        // GET: /Order/Details/
        public IActionResult Details(int orderId)
        {
            AllOrderVM allOrderVM = new AllOrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader
                    .FirstOrDefault(
                        filter: u => u.Id == orderId,
                        includeProperties: "ApplicationUser"
                    ),

                OrderDetail = _unitOfWork.OrderDetail
                    .GetAll(
                        filter: u => u.OrderHeaderId == orderId,
                        includeProperties: "Product"
                    )
            };

            return View(allOrderVM);
        }
        // GET: Shows empty Datewise Orders page with filter form
        public IActionResult DatewiseOrders()
        {
            return View();
        }

        // POST: Called when Search button is clicked
        [HttpPost]
        public IActionResult DatewiseOrders(DateTime fromDate, DateTime toDate, string orderStatus)
        {
            // Include full toDate day (up to 23:59:59)
            toDate = toDate.Date.AddDays(1).AddSeconds(-1);

            IEnumerable<OrderHeader> orders = _unitOfWork.OrderHeader
                .GetAll(includeProperties: "ApplicationUser");

            // Filter by date range
            orders = orders.Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate);

            // Filter by status only if not "All"
            if (!string.IsNullOrEmpty(orderStatus) && orderStatus != "All")
            {
                orders = orders.Where(o => o.OrderStatus == orderStatus);
            }

            // Pass values back to view to retain form state
            ViewBag.FromDate = fromDate.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate.Date.ToString("yyyy-MM-dd");
            ViewBag.Status = orderStatus;
            ViewBag.GrandTotal = orders.Sum(o => o.OrderTotal);
            ViewBag.Searched = true;

            return View(orders);
        }
        public IActionResult MonthlyOrders(int month = 0)
        {
            var months = new Dictionary<int, string>
    {
        {0, "All Months"}, {1,"January"}, {2,"February"}, {3,"March"},
        {4,"April"}, {5,"May"}, {6,"June"}, {7,"July"},
        {8,"August"}, {9,"September"}, {10,"October"},
        {11,"November"}, {12,"December"}
    };

            int currentYear = DateTime.Now.Year;

            // ✅ Fetch all orders with ApplicationUser (matches your existing pattern)
            IEnumerable<OrderHeader> orders = _unitOfWork.OrderHeader
                .GetAll(includeProperties: "ApplicationUser");

            // ✅ Filter by month in-memory (same approach as your DatewiseOrders)
            if (month > 0)
            {
                orders = orders.Where(o => o.OrderDate.Month == month
                                        && o.OrderDate.Year == currentYear);
            }

            // ✅ Sort by date descending
            orders = orders.OrderByDescending(o => o.OrderDate);
            var orderList = orders.ToList();

            var monthlyOrderVM = new MonthlyOrderVM
            {
                SelectedMonth = month,
                SelectedMonthName = months[month],
                SelectedYear = currentYear,
                Orders = orders.ToList(),
                Months = months,
                TotalOrders = orderList.Count(),
                TotalRevenue = orderList.Sum(o => o.OrderTotal)  // ✅ matches your OrderHeader.OrderTotal
            };

            return View(monthlyOrderVM);
        }
    }
}
