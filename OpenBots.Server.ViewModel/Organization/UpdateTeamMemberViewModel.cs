using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.ViewModel.Organization
{
    public class UpdateTeamMemberViewModel
    {
        [Required(ErrorMessage = "Please enter your name.")]
        public string Name { get; set; }

        [RegularExpression("^[A-Za-z0-9_\\+-]+(\\.[A-Za-z0-9_\\+-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*\\.([A-Za-z]{2,4})$", ErrorMessage = "Enter valid Email address.")]
        [StringLength(256, ErrorMessage = "Enter valid Email address.")]
        [Required(ErrorMessage = "Please enter a valid Email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your password.")]
        public string Password { get; set; }
    }
}
