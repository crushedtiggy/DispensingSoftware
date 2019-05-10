using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models
{
    public class Bill_transaction
    {
        public int Bill_transaction_id { get; set; }
        public int Prescription_id { get; set; }
        public int Queue_id { get; set; }
        public int Payment_type { get; set; }
        public double Subtotal { get; set; }
        public DateTime Payment_datetime { get; set; }
    }
}
