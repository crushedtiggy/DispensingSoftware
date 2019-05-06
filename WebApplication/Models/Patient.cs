using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Models;

namespace WebApplication.Models
{
    public class Patient
    {
        public int Patient_id { get; set; }
        public int Queue_id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "NRIC is required")]
        [RegularExpression("^[STFG]\\d{7}[A-Z]$", ErrorMessage = "NRIC is invalid")]
        public string Nric { get; set; }
        public string Gender { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date_of_birth { get; set; }

        public double Height { get; set; }
        public double Weight { get; set; }
        public string Race { get; set; }
        public string Allergy { get; set; }
        public string Smoke { get; set; }
        public string Alcohol { get; set; }
        public string Has_travel { get; set; }
        public string Has_flu { get; set; }
        public string Has_following_symptoms { get; set; }
        public string Address { get; set; }
        public int Postal_code { get; set; }
        public int Phone_no { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string Remarks { get; set; }
        public DateTime Registered_datetime { get; set; }
        public string Is_Urgent { get; set; }
    }
}
