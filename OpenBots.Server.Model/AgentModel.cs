using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model
{
    public class AgentModel : NamedEntity
    {
        [Required]
        public string MachineName { get; set; }
        public string MacAddresses { get; set; }
        public string IPAddresses { get; set; }
        [Required]
        public bool IsEnabled { get; set; }
        public DateTime? LastReportedOn { get; set; }
        public string? LastReportedStatus { get; set; }
        public string? LastReportedWork { get; set; }
        public string? LastReportedMessage { get; set; }
        public bool? IsHealthy { get; set; }
        [Required]
        public bool IsConnected { get; set; }
        public Guid? CredentialId { get; set; }
    }
}
