using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface IQueueItemManager : IManager
    {
        public Task<QueueItemModel> Enqueue(QueueItemModel item);

        public Task<QueueItemModel> Dequeue(string agentId, string queueId);

        public Task<QueueItemModel> Commit(Guid queueItemId, Guid transactionKey, string resultJSON);

        public Task<QueueItemModel> Rollback(Guid queueItemId, Guid transactionKey, int retryLimit, string errorCode = null, string errorMessage = null, bool isFatal = false);

        public Task<QueueItemModel> Extend(Guid queueItemId, Guid transactionKey, int extendByMinutes = 60);

        public Task<QueueItemModel> UpdateState(Guid queueItemId, Guid transactionKey, string state = null, string stateMessage = null, string errorCode = null, string errorMessage = null);

        public Task<QueueItemModel> GetQueueItem(Guid transactionKeyId);
        enum QueueItemStateType : int { };
        PaginatedList<AllQueueItemsViewModel> GetQueueItemsAndBinaryObjectIds(Predicate<AllQueueItemsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        QueueItemViewModel GetQueueItemView(QueueItemViewModel queueItemView, string id);
    }
}