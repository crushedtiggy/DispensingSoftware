using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models
{
    public class Medicine
    {
        public int Medicine_id { get; set; }
        public int Category_type { get; set; }
        public int Subcategory_type { get; set; }
        public int Use_type { get; set; }
        public int Auxiliary_id { get; set; }
        public string Medicine_name { get; set; }
        public string Brand { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public int Threshold { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
    }
}
