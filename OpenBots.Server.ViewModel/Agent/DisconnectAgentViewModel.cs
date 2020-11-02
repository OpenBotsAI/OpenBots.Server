using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel.AgentViewModels
{
    public class DisConnectAgentViewModel
    {
        [Required]
        [FromQuery(Name = "MachineName")]
        public string MachineName { get; set; }
        [Required]
        [FromQuery(Name = "MacAddresses")]
        public string MacAddresses { get; set; }
        [FromQuery(Name = "AgentId")]
        public Guid AgentId { get; set; }
    }
}
