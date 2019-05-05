using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models
{
    public class Prescription
    {
        public int Patient_id { get; set; }
        public int Prescription_id { get; set; }
        public string Doctor_mcr { get; set; }
        public string Doctor_name { get; set; }
        public string Practicing_place_name { get; set; }
        public string Practicing_address { get; set; }
    }
}
