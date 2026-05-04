using System;
using System.Collections.Generic;
using System.Text;

namespace EComm_Project_Models.ViewModels
{
    public class MonthlyOrderVM
    {
        public int SelectedMonth { get; set; }
        public string SelectedMonthName { get; set; }
        public int SelectedYear { get; set; }
        public List<OrderHeader> Orders { get; set; }   // ✅ not generic 'Order'
        public Dictionary<int, string> Months { get; set; }
        public int TotalOrders { get; set; }
        public double TotalRevenue { get; set; }
      

    }
}
