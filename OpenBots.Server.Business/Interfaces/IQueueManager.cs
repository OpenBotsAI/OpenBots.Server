using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;

namespace OpenBots.Server.Business
{
    public interface IQueueManager : IManager
    {
        Queue CheckReferentialIntegrity(string id);

        public Queue UpdateQueue(string id, QueueViewModel request);
    }
}
