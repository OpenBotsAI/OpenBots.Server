using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable enable

namespace OpenBots.Server.Model.Membership
{
    public class OrganizationUnitMember : Entity, ITenanted
    {
        [Required]
        [Display(Name = "OrganizationId")]
        public Guid? OrganizationId { get; set; }
        
        [Required]
        [Display(Name = "OrganizationUnitId")]
        public Guid? OrganizationUnitId { get; set; }
       
        [Required]
        [Display(Name = "PersonId")]
        public Guid? PersonId { get; set; }

        [DefaultValue(false)]
        [Display(Name = "IsAdministrator")]
        public bool? IsAdministrator { get; set; }

        [ForeignKey("OrganizationId")]
        [Display(Name = "Organization")]
        public Organization? Organization { get; set; }

        [ForeignKey("OrganizationUnitId")]
        [Display(Name = "OrganizationUnit")]
        public OrganizationUnit? OrganizationUnit { get; set; }

        [ForeignKey("PersonId")]
        [Display(Name = "Person")]
        public Person? Person { get; set; }
    }
}
