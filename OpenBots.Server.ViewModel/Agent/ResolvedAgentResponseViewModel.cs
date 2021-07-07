using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.ViewModel
{
    public class ResolvedAgentResponseViewModel
    {
        public Guid? AgentId { get; set; }
        public string AgentName { get; set; }
        public string AgentGroupsCS { get; set; }
        public int? HeartbeatInterval { get; set; }
        public int? JobLoggingInterval { get; set; }
        public bool? VerifySslCertificate { get; set; }
    }
}