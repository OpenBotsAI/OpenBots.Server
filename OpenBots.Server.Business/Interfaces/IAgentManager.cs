using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.AgentViewModels;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.Business
{
    public interface IAgentManager : IManager
    {
        void CreateAgentUserAccount(CreateAgentViewModel request);

        void DeleteAgentDependencies(Agent agent);

        Agent UpdateAgent(string id, Agent request);

        void UpdateAgentName(string oldName, string newName);

        AgentViewModel GetAgentDetails(AgentViewModel agentView);
        
        bool CheckReferentialIntegrity(string id);

        Agent GetConnectAgent(string agentId, string requestIp, ConnectAgentViewModel request);
        PaginatedList<AgentGroupMember> GetAllMembersInGroup(string agentId);
    }
}
