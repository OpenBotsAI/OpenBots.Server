using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable enable

namespace OpenBots.Server.Model.Membership
{
    public class OrganizationUnit : Entity, ITenanted
    {
        public OrganizationUnit()
        {
            Members = new List<OrganizationUnitMember>();
            CanDelete = true;
        }

        [Required]
        [Display(Name= "OrganizationId")]
        public Guid? OrganizationId { get; set; }

        [MaxLength(100, ErrorMessage = "Name must be 100 characters or less.")]
        [Required]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters.")]
        [RegularExpression("^[A-Za-z0-9_. ]{3,100}$")] // Alphanumeric with Underscore and Dot only
        [Display(Name = "Name")]
        public string? Name { get; set; }

        [StringLength(500, ErrorMessage = "Description must be 500 characters or less.")]
        [Display(Name= "Description")]
        public string? Description { get; set; }

        [Display(Name= "Is Visible To All OrganizationMembers")]
        public bool? IsVisibleToAllOrganizationMembers { get; set; }

        [DefaultValue(false)]
        [Display(Name="Can Delete")]
        public bool? CanDelete { get; set; }

        [ForeignKey("OrganizationId")]
        [Display(Name="Organization")]
        public Organization? Organization { get; set; }

        public List<OrganizationUnitMember> Members { get; set; }
    }
}
