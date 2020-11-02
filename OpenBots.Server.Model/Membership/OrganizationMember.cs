using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable enable

namespace OpenBots.Server.Model.Membership
{
    public class OrganizationMember : Entity, ITenanted
    {
        [Required]
        [Display(Name = "OrganizationId")]
        public Guid? OrganizationId { get; set; }

        [Required]
        [Display(Name = "PersonId")]
        public Guid? PersonId { get; set; }

        [DefaultValue(false)]
        [Display(Name = "IsAdministrator")]
        public bool? IsAdministrator { get; set; }

        [Display(Name = "ApprovedBy")]
        public string? ApprovedBy { get; set; }

        [Display(Name = "ApprovedOn")]
        public DateTime? ApprovedOn { get; set; }

        [DefaultValue(false)]
        [Display(Name = "IsInvited")]
        public bool? IsInvited { get; set; }

        [Display(Name = "InvitedBy")]
        public string? InvitedBy { get; set; }

        [Display(Name= "InvitedOn")]
        public DateTime? InvitedOn { get; set; }

        [DefaultValue(false)]
        [Display(Name = "InviteAccepted")]
        public bool? InviteAccepted { get; set; }

        [Display(Name = "InviteAcceptedOn")]
        public DateTime? InviteAcceptedOn { get; set; }

        [DefaultValue(false)]
        [Display(Name = "IsAutoApprovedByEmailAddress")]
        public bool? IsAutoApprovedByEmailAddress { get; set; }

        [ForeignKey("PersonId")]
        [Display(Name = "Person")]
        public Person? Person { get; set; }
    }
}
