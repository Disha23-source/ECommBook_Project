using System;
using System.Collections.Generic;
using System.Text;

namespace EComm_Project_DataAccess.Repository.IRepository
{
    public interface ITwilioServices
    {
        Task MakeOrderConfirmationCallAsync(string toPhoneNumber, int orderId, string customerName, IEnumerable<string> productNames);
    }
}
