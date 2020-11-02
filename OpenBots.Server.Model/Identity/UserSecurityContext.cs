using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
#nullable enable

namespace OpenBots.Server.Model.Identity
{
    public class UserSecurityContext
    {
        public UserSecurityContext()
        {
            OrganizationUnits = new Dictionary<Guid, bool>();
            UserInRoles = new List<string>();
        }

        [Display(Name = "PersonId")]
        public Guid PersonId { get; set; }

        [Display(Name = "OrganizationId")]
        public Guid[] OrganizationId { get; set; }

        [Display(Name = "IsOrganizationAdmin")]
        public bool IsOrganizationAdmin { get; set; }

        [Display(Name = "OrganizationUnits")]
        public Dictionary<Guid, bool> OrganizationUnits { get; set; }

        [Display(Name = "UserInRoles")]
        public List<string> UserInRoles { get; set; }
    }
}
