using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.AgentViewModels;
using System;

namespace OpenBots.Server.Business
{
    public interface IAgentManager : IManager
    {
        void CreateAgentUserAccount(CreateAgentViewModel request);

        void DeleteAgentDependencies(Agent agent);

        Agent UpdateAgent(string id, UpdateAgentViewModel request);

        void UpdateAgentName(string oldName, string newName);

        AgentViewModel GetAgentDetails(AgentViewModel agentView);
        
        bool CheckReferentialIntegrity(string id);

        ConnectedViewModel ConnectAgent(string agentId, string requestIp, ConnectAgentViewModel request);

        void DisconnectAgent(Guid? agentId, string requestIp, ConnectAgentViewModel request);

        PaginatedList<AgentGroupMember> GetAllMembersInGroup(string agentId);

        AgentHeartbeat PerformAgentHeartbeat(HeartbeatViewModel request, string agentId);

        NextJobViewModel GetNextJob(string agentId);
    }
}
