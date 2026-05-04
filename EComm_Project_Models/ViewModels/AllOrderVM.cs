using System;
using System.Collections.Generic;
using System.Text;

namespace EComm_Project_Models.ViewModels
{
    public class AllOrderVM
    {
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<OrderDetail> OrderDetail { get; set; }
    }
}
