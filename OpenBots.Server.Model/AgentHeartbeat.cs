using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.Model
{
    /// <summary>
    /// Stores the heartbeat values for the specified Agent ID
    /// </summary>
    public class AgentHeartbeat : Entity, INonAuditable
    {
        [Required]
        [Display(Name = "AgentId")]
        public Guid? AgentId { get; set; }

        public DateTime? LastReportedOn { get; set; }

        public string? LastReportedStatus { get; set; }

        public string? LastReportedWork { get; set; }

        public string? LastReportedMessage { get; set; }

        public bool? IsHealthy { get; set; }
    }
}
