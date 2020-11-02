using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenBots.Server.Model.Identity
{
    public class UserConsent : Entity
    {
        [Required]
        [Display(Name = "PersonId")]
        public Guid? PersonId { get; set; }

        [ForeignKey("PersonId")]
        [Display(Name = "Person")]
        public Person? Person { get; set; }

        [Required]
        [Display(Name = "UserAgreementID")]
        public Guid? UserAgreementID { get; set; }
        
        [Required]
        [Display(Name = "IsAccepted")]
        public bool IsAccepted { get; set; }

        [ForeignKey("UserAgreementID")]
        [Display(Name = "UserAgreements")]
        public UserAgreement? UserAgreements { get; set; }

        [Display(Name = "RecordedOn")]
        public DateTime? RecordedOn { get; set; }

        [Display(Name = "ExpiresOnUTC")]
        public DateTime? ExpiresOnUTC { get; set; }
    }
}
