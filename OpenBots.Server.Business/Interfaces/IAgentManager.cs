using OpenBots.Server.ViewModel;

namespace OpenBots.Server.Business
{
    public interface IAgentManager : IManager
    {
        AgentViewModel GetAgentDetails(AgentViewModel agentView);
        
        bool CheckReferentialIntegrity(string id);
    }
}
