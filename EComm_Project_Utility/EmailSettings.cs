using System;
using System.Collections.Generic;
using System.Text;

namespace EComm_Project_Utility
{
    public class EmailSettings
    {
        public String PrimaryDomain { get; set; }
        public int PrimaryPort { get; set; }
        public String SecondaryDomain { get; set; }
        public int SecondaryPort { get; set; }
        public String UserNameEmail { get; set; }
        public String UserNamePassword { get; set; }
        public String FromEmail {  get; set; }
        public String ToEmail { get; set; }
        public String CcEmail {  get; set; }
    }
}