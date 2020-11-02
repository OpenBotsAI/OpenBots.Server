using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable enable

namespace OpenBots.Server.Model.Membership
{
    public class OrganizationSetting : Entity, ITenanted
    {
        public OrganizationSetting()
        {
        }

        [Display(Name= "OrganizationId")]
        public Guid? OrganizationId { get; set; }
        [ForeignKey("OrganizationId")]
        public Organization? Organization { get; set; }
        [Display(Name = "TimeZone")]
        public string? TimeZone { get; set; }
        [Display(Name = "StorageLocation")]
        public string? StorageLocation { get; set; }
    }
}