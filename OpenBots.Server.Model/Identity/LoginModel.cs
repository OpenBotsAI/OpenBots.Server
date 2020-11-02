using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBots.Server.Model.Identity
{
    [NotMapped]
    public class LoginModel
    {
        [Required(ErrorMessage = "Please enter your username.")]
        public String UserName { get; set; }

        [Required]
        public String Password { get; set; }
    }
}
