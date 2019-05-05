using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models
{
    public class Queue
    {
        public int Queue_id { get; set; }
        public int Patient_id { get; set; }
        public int Serve_status_id { get; set; }
        public int Queue_category_id { get; set; }
        public DateTime Queue_datetime { get; set; }
    }
}
