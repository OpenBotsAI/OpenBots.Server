using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.Model
{
    public class IPFencing : Entity
    {
        [Required]
        [Display(Name = "Usage")]
        public UsageType? Usage { get; set; }

        [Required]
        [Display(Name = "Rule")]
        public RuleType? Rule { get; set; }

        [Display(Name = "IPAddress")]
        public string? IPAddress { get; set; }

        [Display(Name = "IPRange")]
        public string? IPRange { get; set; }

        [Display(Name = "HeaderName")]
        public string? HeaderName { get; set; }

        [Display(Name = "HeaderValue")]
        public string? HeaderValue { get; set; }

        [Display(Name = "OrganizationId")]
        public Guid? OrganizationId { get; set; }
    }

    /// <summary>
    /// Stores the usage type for the IPFencing.
    /// </summary>
    /// <remarks>
    /// If usage type is Deny, then all IPs except the ones specified are denied. <br/>
    /// If usage type is Allowed, then all IPs except the ones specified are allowed.
    /// </remarks>
    public enum UsageType : int
    {
        Allow = 1,
        Deny = -1
    }

    /// <summary>
    /// Represents the type of rule that is being stored
    /// </summary>
    /// <remarks>
    /// IP rule values can be stored as individual addresses or as ranges. <br/>
    /// Headers are checked after IP values and must be exist in the http request
    /// </remarks>
    public enum RuleType : int
    {
        IPv4 = 1,
        IPv4Range = 2,
        IPv6 = 3,
        IPv6Range = 4,
        Header = 5
    }
}
