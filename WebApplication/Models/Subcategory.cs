using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models
{
    public class Subcategory
    {
        public int Subcategory_type { get; set; }
        public string Subcategory_name { get; set; }
        public int Category_type { get; set; }
    }
}
