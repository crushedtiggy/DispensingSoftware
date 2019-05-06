using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication.Models;

namespace WebApplication.Models
{
    public class Patient
    {
        public int Patient_id { get; set; }
        public int Queue_id { get; set; }
        public string Name { get; set; }
        public string Nric { get; set; }
        public string Gender { get; set; }
        public string Date_of_birth { get; set; }
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
        public string Email { get; set; }
        public string Remarks { get; set; }
        public string Registered_datetime { get; set; }
        public string Is_Urgent { get; set; }
    }
}
