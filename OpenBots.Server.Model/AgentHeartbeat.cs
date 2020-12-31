using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model
{
    /// <summary>
    /// Stores the heartbeat values for the specified Agent ID
    /// </summary>
    public class AgentHeartbeat : Entity, INonAuditable
    {
        [Display(Name = "AgentId")]
        public Guid AgentId { get; set; }

        [Display(Name = "LastReportedOn")]
        public DateTime? LastReportedOn { get; set; }

        [Display(Name = "LastReportedStatus")]
        public string? LastReportedStatus { get; set; }

        [Display(Name = "LastReportedWork")]
        public string? LastReportedWork { get; set; }

        [Display(Name = "LastReportedMessage")]
        public string? LastReportedMessage { get; set; }

        [Display(Name = "IsHealthy")]
        public bool? IsHealthy { get; set; }
    }
}
