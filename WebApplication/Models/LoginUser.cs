using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models
{
    public class LoginUser
    {
        [Required(ErrorMessage = "User ID cannot be empty!")]
        public string Login_name { get; set; }

        [Required(ErrorMessage = "Empty password not allowed!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public int Id { get; set; }
        public string User_name { get; set; }
    }
}
