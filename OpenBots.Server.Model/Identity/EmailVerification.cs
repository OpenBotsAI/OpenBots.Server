using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable enable

namespace OpenBots.Server.Model.Identity
{
    public class EmailVerification : Entity
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

        [DefaultValue(false)]
        [Display(Name = "IsVerified")]
        public bool? IsVerified { get; set; }

        [Display(Name = "VerificationEmailCount")]
        public int VerificationEmailCount { get; set; }

        [Display(Name = "VerificationCode")]
        [StringLength(100,ErrorMessage ="Verification code must be 100 characters or less")]
        public string? VerificationCode { get; set; }

        [Display(Name = "VerificationCodeExpiresOn")]
        public DateTime? VerificationCodeExpiresOn { get; set; }

        [DefaultValue(false)]
        [Display(Name = "IsVerificationEmailSent")]
        public bool? IsVerificationEmailSent { get; set; }

        [Display(Name = "VerificationSentOn")]
        public DateTime? VerificationSentOn { get; set; }

        [ForeignKey("PersonId")]
        [Display(Name = "Person")]
        public Person? Person { get; set; }
    }
}
