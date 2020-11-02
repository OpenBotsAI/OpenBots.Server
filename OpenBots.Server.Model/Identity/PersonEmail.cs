using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable enable

namespace OpenBots.Server.Model.Identity
{
    public class PersonEmail : Entity
    {
        [Required]
        [Display(Name = "PersonId")]
        public Guid? PersonId { get; set; }

        [RegularExpression("^[A-Za-z0-9_\\+-]+(\\.[A-Za-z0-9_\\+-]+)*@[A-Za-z0-9-]+(\\.[A-Za-z0-9]+)*\\.([A-Za-z]{2,4})$", ErrorMessage = "Enter valid Email address.")]
        [StringLength(256, ErrorMessage = "Enter valid Email address.")]
        [Required(ErrorMessage = "Please enter your e-mail address.")]
        [EmailAddress(ErrorMessage = "Enter valid Email address.")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Required]
        [Display(Name = "EmailVerificationId")]
        public Guid? EmailVerificationId { get; set; }

        [ForeignKey("PersonId")]
        [Display(Name = "Person")]
        public Person? Person { get; set; }

        [Required]
        [Display(Name = "PrimaryEmail")]
        public bool IsPrimaryEmail { get; set; }
    }
}
