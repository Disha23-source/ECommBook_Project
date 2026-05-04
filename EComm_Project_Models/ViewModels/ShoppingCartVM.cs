using System;
using System.Collections.Generic;
using System.Text;

namespace EComm_Project_Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ListCart {  get; set; }
        public OrderHeader OrderHeader { get; set; }
        public bool IsSelected { get; set; }
        public IEnumerable<UserAddress> UserAddresses { get; set; }
    }
}

