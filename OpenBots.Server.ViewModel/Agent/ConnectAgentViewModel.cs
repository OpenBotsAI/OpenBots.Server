using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel.AgentViewModels
{
    public class ConnectAgentViewModel
    {
        [Required]
        [FromQuery(Name = "MachineName")]
        public string MachineName { get; set; }
        [Required]
        [FromQuery(Name = "MacAddresses")]
        public string MacAddresses { get; set; }
    }
}
