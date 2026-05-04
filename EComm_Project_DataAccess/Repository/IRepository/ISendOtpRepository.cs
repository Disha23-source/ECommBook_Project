using System;
using System.Collections.Generic;
using System.Text;

namespace EComm_Project_DataAccess.Repository.IRepository
{
    public interface ISendOtpRepository
    {
        Task SendSmsAsync(string number, string message);
    }
}
