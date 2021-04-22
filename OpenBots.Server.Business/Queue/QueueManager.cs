using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using System;
using System.Linq;

namespace OpenBots.Server.Business
{
    public class QueueManager : BaseManager, IQueueManager
    {
        private readonly IQueueRepository _queueRepo;
        private readonly IQueueItemRepository _queueItemRepo;

        public QueueManager(IQueueRepository queueRepository, IQueueItemRepository queueItemRepository)
        {
            _queueItemRepo = queueItemRepository;
            _queueRepo = queueRepository;
        }

        public Queue CheckReferentialIntegrity(string id)
        {
            Guid entityId = new Guid(id);
            var existingQueue = _queueRepo.GetOne(entityId);
            if (existingQueue == null) throw new EntityDoesNotExistException("Queue could not be found");

            var lockedQueueItems = _queueItemRepo.Find(0, 1).Items?
                .Where(q => q.QueueId == entityId && q.IsLocked);

            bool lockedChildExists = lockedQueueItems.Count() > 0;
            if (lockedChildExists)
                throw new EntityOperationException("Referential integrity in queue items table; please remove any locked items associated with this queue first");

            return existingQueue;
        }

        public Queue UpdateQueue(string id, QueueViewModel request)
        {
            Guid entityId = new Guid(id);

            var existingQueue = _queueRepo.GetOne(entityId);
            if (existingQueue == null) throw new EntityDoesNotExistException("Queue could not be found");

            var queue = _queueRepo.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null) && d.Id != entityId)?.Items?.FirstOrDefault();
            if (queue != null && existingQueue.Id != entityId) throw new EntityAlreadyExistsException("Queue already exists with same name");

            existingQueue.Description = request.Description;
            existingQueue.Name = request.Name;
            existingQueue.MaxRetryCount = request.MaxRetryCount;

            return existingQueue;
        }
    }
}
