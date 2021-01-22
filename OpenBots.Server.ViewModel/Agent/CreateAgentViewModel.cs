using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel.AgentViewModels
{
    public class CreateAgentViewModel : IViewModel<CreateAgentViewModel, Agent>
    {
        public Guid? Id { get; set; }
        [RegularExpression("^[A-Za-z0-9_.-]{3,100}$", ErrorMessage = "Please enter valid Agent name.")] //alphanumeric with underscore, hyphen and dot only
        [Required(ErrorMessage = "Please enter an agent name.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please enter a Machine name.")]
        public string MachineName { get; set; }
        public string MacAddresses { get; set; }
        public string IPAddresses { get; set; }
        [Required]
        public bool IsEnabled { get; set; }
        [Required]
        public bool IsConnected { get; set; }
        public Guid? CredentialId { get; set; }
        [RegularExpression("^[A-Za-z0-9_.-]{3,100}$", ErrorMessage = "Enter valid UserName.")] // alphanumeric with underscore, hyphen and dot only
        [Required(ErrorMessage = "Please enter your username.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please enter your password.")]
        public string Password { get; set; }
        public string IPOption { get; set; }
        public bool IsEnhancedSecurity { get; set; }

        public Agent Map(CreateAgentViewModel viewModel)
        {
            Agent agent = new Agent
            {
                Id = viewModel.Id,
                Name = viewModel.Name,
                MachineName = viewModel.MachineName,
                MacAddresses = viewModel.MacAddresses,
                IPAddresses = viewModel.IPAddresses,
                IsEnabled = viewModel.IsEnabled,
                IsConnected = viewModel.IsConnected,
                CredentialId = viewModel.CredentialId,
                IPOption = viewModel.IPOption,
                IsEnhancedSecurity = viewModel.IsEnhancedSecurity
            };

            return agent;
        }
    }
}
