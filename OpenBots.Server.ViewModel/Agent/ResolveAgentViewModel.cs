using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.ViewModel
{
    public class ResolveAgentViewModel
    {
        public string AgentGroupName { get; set; }
        public string AgentName { get; set; }
        [Required]
        public string HostMachineName { get; set; }
        public string MacAddressesCS { get; set; }
    }
}
