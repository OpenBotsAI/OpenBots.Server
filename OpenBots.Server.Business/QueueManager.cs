using OpenBots.Server.DataAccess.Repositories;
using System;
using System.Linq;

namespace OpenBots.Server.Business
{
    public class QueueManager : BaseManager, IQueueManager
    {
        private readonly IQueueItemRepository queueItemRepo;
        public QueueManager(IQueueItemRepository queueItemRepository)
        {
            queueItemRepo = queueItemRepository;
        }

        public bool CheckReferentialIntegrity(string id)
        {
            Guid queueId = new Guid(id);

            var lockedQueueItems = queueItemRepo.Find(0, 1).Items?
                .Where(q => q.QueueId == queueId && q.IsLocked);

            return lockedQueueItems.Count() > 0;
        }
    }
}
