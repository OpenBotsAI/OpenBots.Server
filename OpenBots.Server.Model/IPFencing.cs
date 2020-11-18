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
        public UsageType? Usage { get; set; }
        public RuleType? Rule { get; set; }
        public string? IPAddress { get; set; }
        public string? IPRange { get; set; }
        public string? HeaderName { get; set; }
        public string? HeaderValue { get; set; }
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
    /// Re 
    /// </summary>
    public enum RuleType : int
    {
        IPv4 = 1,
        IPv4Range = 2,
        IPv6 = 3,
        IPv6Range = 4,
        Header = 5
    }
}
