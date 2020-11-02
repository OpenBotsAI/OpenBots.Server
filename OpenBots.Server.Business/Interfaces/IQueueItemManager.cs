using OpenBots.Server.Model;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface IQueueItemManager : IManager
    {
        public Task<QueueItem> Enqueue(QueueItem item);

        public Task<QueueItem> Dequeue(string agentId, string queueId);

        public Task Commit(Guid queueItemId, Guid transactionKey);

        public Task Rollback(Guid queueItemId, Guid transactionKey, int retryLimit, string errorCode = null, string errorMessage = null, bool isFatal = false);

        public Task Extend(Guid queueItemId, Guid transactionKey, int extendByMinutes = 60);

        public Task UpdateState(Guid queueItemId, Guid transactionKey, string state = null, string stateMessage = null, string errorCode = null, string errorMessage = null);

        public Task<QueueItem> GetQueueItem(Guid transactionKeyId);
    }
}