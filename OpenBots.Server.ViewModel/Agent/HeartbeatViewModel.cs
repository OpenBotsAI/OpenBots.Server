using System;

namespace OpenBots.Server.ViewModel.AgentViewModels
{
    public class HeartbeatViewModel
    {
        public DateTime? LastReportedOn { get; set; }

        public string? LastReportedStatus { get; set; }

        public string? LastReportedWork { get; set; }

        public string? LastReportedMessage { get; set; }

        public bool? IsHealthy { get; set; }
    }
}
