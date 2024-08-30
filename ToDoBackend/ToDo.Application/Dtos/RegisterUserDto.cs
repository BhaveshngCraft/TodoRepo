using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDo.Application.Dtos
{
    public class RegisterUserDto
    {

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty ;
        public string ConfirmPassword { get; set; } = string.Empty;
        
    }

    public class LoginUserDto
    {

        public string Username { get; set; } = string.Empty;     
        public string Password { get; set; } = string.Empty;

    }
}
