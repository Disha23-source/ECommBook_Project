using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace EComm_Project_Utility
{
    public static class SD
    {
        //Roles
        public const string Role_Admin = "Admin User";
        public const string Role_Employee = "Employee User";
        public const string Role_Company = "Company User";
        public const string Role_Individual = "Individual User";
        //Session Variable
        public const string Ss_CartSessionCount = "Cart Count Session";
        //ShoppingCart -- to get the price of the product by Quantity
        public static double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity < 50)
                return price;
            else if (quantity < 100)
                return price50; return price100;
        }
        //Order Status
        public const string OrderStatusPending = "Pending";
        public const string OrderStatusApproved = "Approved";
        public const string OrderStatusInProgress = "Processing";
        public const string OrderStatusShipped = "Shipped";
        public const string OrderStatusCancelled = "Cancelled";
        public const string OrderStausRefunded = "Refunded";
        //Payment Status
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedPayment = "PaymentStatusDelay";
        public const string PaymentStatusRejected = "Rejected";
    }
}
