using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using System;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface IAgentRepository : IEntityRepository<AgentModel>
    {
        AgentModel FindAgent(string machineName, string macAddress, string ipAddress, Guid? agentID);

        AgentViewModel GetAgentDetailById(string id);
    }
}
