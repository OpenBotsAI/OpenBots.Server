using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class AgentViewModel : IViewModel<AgentModel, AgentViewModel>
    {
        [Display(Name = "Id")]
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
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
        public string CredentialName { get; set; }
        public AgentViewModel Map(AgentModel entity)
        {
            AgentViewModel agentView = new AgentViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                MachineName = entity.MachineName,
                MacAddresses = entity.MacAddresses,
                IPAddresses = entity.IPAddresses,
                IsEnabled = entity.IsEnabled,
                LastReportedOn = entity.LastReportedOn,
                LastReportedStatus = entity.LastReportedStatus,
                LastReportedWork = entity.LastReportedWork,
                LastReportedMessage = entity.LastReportedMessage,
                IsHealthy = entity.IsHealthy,
                IsConnected = entity.IsConnected,
                CredentialId = entity.CredentialId
            };

            return agentView;
        }
    }
}
