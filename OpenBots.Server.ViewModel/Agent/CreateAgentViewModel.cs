using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel.AgentViewModels
{
    public class CreateAgentViewModel : IViewModel<CreateAgentViewModel, AgentModel>
    {
        public Guid? Id { get; set; }
        [RegularExpression("^[A-Za-z0-9_.-]{3,100}$", ErrorMessage = "Please enter valid Agent name.")] // Alphanumeric with Underscore, Hyphen and Dot only
        [Required(ErrorMessage = "Please enter an agent name.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please enter a Machine name.")]
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
        public bool isConnected { get; set; }
        public Guid? CredentialId { get; set; }
        [RegularExpression("^[A-Za-z0-9_.-]{3,100}$", ErrorMessage = "Enter valid UserName.")] // Alphanumeric with Underscore, Hyphen and Dot only
        [Required(ErrorMessage = "Please enter your username.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please enter your password.")]
        public string Password { get; set; }
        public AgentModel Map(CreateAgentViewModel viewModel)
        {
            AgentModel agent = new AgentModel
            {
                Id = viewModel.Id,
                Name = viewModel.Name,
                MachineName = viewModel.MachineName,
                MacAddresses = viewModel.MacAddresses,
                IPAddresses = viewModel.IPAddresses,
                IsEnabled = viewModel.IsEnabled,
                LastReportedOn = viewModel.LastReportedOn,
                LastReportedStatus = viewModel.LastReportedStatus,
                LastReportedWork = viewModel.LastReportedWork,
                LastReportedMessage = viewModel.LastReportedMessage,
                IsHealthy = viewModel.IsHealthy,
                IsConnected = viewModel.isConnected,
                CredentialId = viewModel.CredentialId
            };

            return agent;
        }
    }
}
