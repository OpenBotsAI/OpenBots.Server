using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Security.ViewModel
{
    public class SetPasswordBindingModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }
    }
}
