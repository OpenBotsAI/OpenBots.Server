using System;

namespace OpenBots.Server.ViewModel.AgentViewModels
{
    public class AllAgentsViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string MachineName { get; set; }
        public string MacAddresses { get; set; }
        public string IPAddresses { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? LastReportedOn { get; set; }
        public string? LastReportedStatus { get; set; }
        public string? LastReportedWork { get; set; }
        public string? LastReportedMessage { get; set; }
        public bool? IsHealthy { get; set; }
        public string Status { get; set; }
        public Guid? CredentialId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string IPOption { get; set; }
        public bool IsEnhancedSecurity { get; set; }
    }
}
