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

        [Display(Name = "IPFencingMode")]
        public IPFencingMode? IPFencingMode { get; set; }

        [Display(Name = "DisallowAllExecutions")]
        public bool DisallowAllExecutions { get; set; }

        [Display(Name = "DisallowAllExecutionsReason ")]
        public string? DisallowAllExecutionsReason { get; set; }

        [Display(Name = "DisallowAllExecutionsMessage ")]
        public string? DisallowAllExecutionsMessage { get; set; }
    }
    public enum IPFencingMode : int
    {
        AllowMode = 1,
        DenyMode = -1
    }
}