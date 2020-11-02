using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.Core
{
    public class LookupValue : Entity
    {
        [Required]
        [Display(Name = "LookupCode")]
        public string LookupCode { get; set; }

        [Display(Name = "LookupDesc")]
        public string? LookupDesc { get; set; }

        [Required]
        [Display(Name = "CodeType")]
        public string CodeType { get; set; }

        [Display(Name = "OrganizationId")]
        public Guid? OrganizationId { get; set; }

        [Display(Name = "SequenceOrder")]
        public int? SequenceOrder { get; set; }
    }
}
