using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models
{
    public class User
    {
        public int User_id { get; set; }
        public int Role_id { get; set; }
        public string User_name { get; set; }
        public string Login_name { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public int Postal_code { get; set; }
        public int Contact_no { get; set; }
        public string Email { get; set; }
    }
}
