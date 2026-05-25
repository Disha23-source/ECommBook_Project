using System;
using System.Collections.Generic;
using System.Text;

namespace EComm_Project_Models.ViewModels
{
    public class HomeVM
    {
            public IEnumerable<Product> ProductList { get; set; }
            public string SearchBy { get; set; } = "All";
            public string SearchText { get; set; } = "";
            public int? CategoryId { get; set; }
            public IEnumerable<Category> CategoryList { get; set; }
        
    }
}
