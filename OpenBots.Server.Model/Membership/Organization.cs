using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
#nullable enable

namespace OpenBots.Server.Model.Membership
{
    
    public class Organization : Entity, ITenanted
    {
        public Organization()
        {
            Units = new List<OrganizationUnit>();
            AccessRequests = new List<AccessRequest>();
            Members = new List<OrganizationMember>();
            Settings = new List<OrganizationSetting>();
        }

        [MaxLength(100, ErrorMessage = "Name must be 100 characters or less.")]
        [Required]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters.")]
        [RegularExpression("^[A-Za-z0-9_. ]{3,100}$")] // Alphanumeric with Underscore and Dot only
        [Display(Name = "Name")]
        public string? Name { get; set; }

        [StringLength(500,ErrorMessage ="Description must be 500 characters or less.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
        
        public List<OrganizationUnit> Units { get; set; }

        public List<AccessRequest> AccessRequests { get; set; }

        public List<OrganizationMember> Members { get; set; }

        public List<OrganizationSetting> Settings { get; set; }

        [NotMapped]
        public Guid? OrganizationId { get; set; }
    }
}
