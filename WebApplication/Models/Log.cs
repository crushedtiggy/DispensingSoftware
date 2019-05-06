using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models
{
    public class Log
    {
        public int Log_id { get; set; }
        public int Patient_id { get; set; }
        public int Medicine_id { get; set; }
        public int Dosage_id { get; set; }
        public DateTime Booking_appointment { get; set; }
        public string  Case_notes { get; set; }
        public int Duration { get; set; }
        public int Dosage_quantity { get; set; }
        public string Instructions { get; set; }
        public double Total_price { get; set; }
    }
}
