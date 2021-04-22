using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel.AgentViewModels
{
    public class HeartbeatViewModel : IViewModel<HeartbeatViewModel, AgentHeartbeat>
    {
        public DateTime? LastReportedOn { get; set; }

        public string? LastReportedStatus { get; set; }

        public string? LastReportedWork { get; set; }

        public string? LastReportedMessage { get; set; }

        public bool IsHealthy { get; set; }
        public bool GetNextJob { get; set; }

        public AgentHeartbeat Map(HeartbeatViewModel entity)
        {
            AgentHeartbeat heartbeatModel = new AgentHeartbeat
            {
                LastReportedOn = entity.LastReportedOn,
                LastReportedStatus = entity.LastReportedStatus,
                LastReportedWork = entity.LastReportedWork,
                LastReportedMessage = entity.LastReportedMessage,
                IsHealthy = entity.IsHealthy
            };

            return heartbeatModel;
        }
    }
}
