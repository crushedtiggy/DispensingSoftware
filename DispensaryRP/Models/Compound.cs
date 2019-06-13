using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models
{
    public class Compound
    {
        public int Compound_id { get; set; }
        public int Compound_category_type { get; set; }
        public int Compound_subcategory_type { get; set; }
        public string Compound_name { get; set; }
        public string Strength { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date_prepared { get; set; }
        public string Source_formula { get; set; }
        public int Order_no { get; set; }
        public string Instructions { get; set; }
        [DataType(DataType.Date)]
        public DateTime Expiry_date { get; set; }
        public string Ingredient1 { get; set; }
        [DataType(DataType.Date)]
        public DateTime Ingredient1_expiry_date { get; set; }
        public int Ingredient1_quantity { get; set; }
        public string Ingredient2 { get; set; }
        [DataType(DataType.Date)]
        public DateTime Ingredient2_expiry_date { get; set; }
        public int Ingredient2_quantity { get; set; }
        public string Ingredient3 { get; set; }
        [DataType(DataType.Date)]
        public DateTime Ingredient3_expiry_date { get; set; }
        public int Ingredient3_quantity { get; set; }
        public string Ingredient4 { get; set; }
        [DataType(DataType.Date)]
        public DateTime Ingredient4_expiry_date { get; set; }
        public int Ingredient4_quantity { get; set; }
        public int Quantity_used_total { get; set; }
        public string Student_name { get; set; }
        public string Lecturer_name { get; set; }
        public int Batch_no { get; set; }
        public double Calculation { get; set; }
        public string Procedure { get; set; }
        public string Label { get; set; }
        public string Label_prepared_by { get; set; }
        [DataType(DataType.Date)]
        public DateTime Prepared_date { get; set; }
        public string Label_checked_by { get; set; }
        [DataType(DataType.Date)]
        public DateTime Checked_date { get; set; }
    }
}
