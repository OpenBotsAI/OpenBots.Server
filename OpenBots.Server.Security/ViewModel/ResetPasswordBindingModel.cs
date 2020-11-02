using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Security.ViewModel
{
    public class ResetPasswordBindingModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string NewPassword { get; set; }

        [Required]
        public string Token { get; set; }
    }
}
