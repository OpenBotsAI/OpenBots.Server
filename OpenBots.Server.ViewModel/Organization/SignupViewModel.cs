using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.Identity
{
    /// <summary>
    /// Represents a Person
    /// </summary>
    /// <seealso cref="NamedEntity" />
    public class SignUpViewModel
    {
        public SignUpViewModel()
        {
        }

        [Required(ErrorMessage = "Please enter your name.")]
        public string Name { get; set; }

        public string Organization { get; set; }

        public string? Department { get; set; }

        [RegularExpression("^[A-Za-z0-9_\\+-]+(\\.[A-Za-z0-9_\\+-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*\\.([A-Za-z]{2,4})$", ErrorMessage = "Enter valid Email address.")]
        [StringLength(256, ErrorMessage = "Enter valid Email address.")]
        [Required(ErrorMessage = "Please enter a valid Email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your password.")] 
        public string Password { get; set; }
    }

    public class InviteUserViewModel
    {
        public InviteUserViewModel() {
            SkipEmailVerification = false;
        }

        public Guid? OrganizationId { get; set; }

        public Guid? ProcessId { get; set; }

        [RegularExpression("^[A-Za-z0-9_\\+-]+(\\.[A-Za-z0-9_\\+-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*\\.([A-Za-z]{2,4})$", ErrorMessage = "Enter valid Email address.")]
        [StringLength(256, ErrorMessage = "Enter valid Email address.")]
        [Required(ErrorMessage = "Please enter your e-mail address.")]
        [EmailAddress(ErrorMessage = "Enter valid Email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your name.")]
        [MaxLength(100, ErrorMessage = "Enter maximum 100 characters as name")]
        public string Name { get; set; }

        public string Company { get; set; }

        public string Department { get; set; }

        public string ShareUrl { get; set; }

        public string Password { get; set; }

        public bool SkipEmailVerification { get; set; }
    }
}
