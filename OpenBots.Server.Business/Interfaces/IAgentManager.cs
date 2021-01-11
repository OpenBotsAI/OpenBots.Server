using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.AgentViewModels;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.Business
{
    public interface IAgentManager : IManager
    {
        AgentViewModel GetAgentDetails(AgentViewModel agentView);
        
        bool CheckReferentialIntegrity(string id);

        IEnumerable<AgentHeartbeat> GetAgentHeartbeats(Guid agentId);

        void DeleteExistingHeartbeats(Guid agentId);

        Agent GetConnectAgent(string agentId, string requestIp, ConnectAgentViewModel request);
    }
}
