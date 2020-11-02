using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable enable

namespace OpenBots.Server.Model.Membership
{
    public class AccessRequest : Entity, ITenanted
    {
        [Required]
        [Display(Name = "OrganizationId")]
        public Guid? OrganizationId { get; set; }

        [Required]
        [Display(Name = "PersonId")]
        public Guid? PersonId { get; set; }

        [DefaultValue(false)]
        [Display(Name = "IsAccessRequested")]
        public bool? IsAccessRequested { get; set; }

        [Display(Name = "AccessRequestedOn")]
        public DateTime? AccessRequestedOn { get; set; }

        [ForeignKey("OrganizationId")]
        [Display(Name = "Organization")]
        public Organization? Organization { get; set; }

        [ForeignKey("PersonId")]
        [Display(Name = "Person")]
        public Person? Person { get; set; }
    }
}
