using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;

namespace OpenBots.Server.ViewModel.AgentViewModels
{
    public class ConnectedViewModel : IViewModel<Agent, ConnectedViewModel>
    {
        public string AgentId { get; set; }

        public string AgentName { get; set; }

        public ConnectedViewModel Map(Agent entity)
        {
            ConnectedViewModel connected = new ConnectedViewModel
            {
                AgentId = entity.Id.ToString(),
                AgentName = entity.Name
            };

            return connected;
        }
    }
}
