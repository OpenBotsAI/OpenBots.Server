using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface IAgentRepository : IEntityRepository<AgentModel>
    {
        AgentModel FindAgent(string machineName ,string macAddress, string ipAddress);

        AgentViewModel GetAgentDetailById(string id);
    }
}
