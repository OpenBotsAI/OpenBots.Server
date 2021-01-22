using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBots.Server.Model.Identity
{
    [NotMapped]
    public class Login
    {
        [Required(ErrorMessage = "Please enter your username.")]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
