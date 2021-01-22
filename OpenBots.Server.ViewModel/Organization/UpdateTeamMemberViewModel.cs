using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel.Organization
{
    public class UpdateTeamMemberViewModel
    {
        public string Name { get; set; }

        [RegularExpression("^[A-Za-z0-9_\\+-]+(\\.[A-Za-z0-9_\\+-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*\\.([A-Za-z]{2,4})$", ErrorMessage = "Enter valid Email address.")]
        [StringLength(256, ErrorMessage = "Enter valid Email address.")]
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
