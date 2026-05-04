using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace EComm_Project_Models.ViewModels
{
    public class ProductVM
    {
          public IEnumerable <SelectListItem> CategoryList { get; set; }
        public IEnumerable <SelectListItem> CoverTypeList { get;set; }
        public Product Product { get; set; }
    }
}