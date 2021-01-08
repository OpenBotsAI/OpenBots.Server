using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.Queue;
using OpenBots.Server.ViewModel.QueueItem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface IQueueItemManager : IManager
    {
        public Task<QueueItem> Enqueue(QueueItem item);

        public Task<QueueItem> Dequeue(string agentId, string queueId);

        public Task<QueueItem> Commit(Guid queueItemId, Guid transactionKey, string resultJSON);

        public Task<QueueItem> Rollback(Guid queueItemId, Guid transactionKey, int retryLimit, string errorCode = null, string errorMessage = null, bool isFatal = false);

        public Task<QueueItem> Extend(Guid queueItemId, Guid transactionKey, int extendByMinutes = 60);

        public Task<QueueItem> UpdateState(Guid queueItemId, Guid transactionKey, string state = null, string stateMessage = null, string errorCode = null, string errorMessage = null);

        public Task<QueueItem> GetQueueItem(Guid transactionKeyId);

        enum QueueItemStateType : int { };

        PaginatedList<AllQueueItemsViewModel> GetQueueItemsAndBinaryObjectIds(Predicate<AllQueueItemsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);

        public PaginatedList<AllQueueItemAttachmentsViewModel> GetQueueItemAttachmentsAndNames(Guid queueItemId, Predicate<AllQueueItemAttachmentsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        
        QueueItemViewModel GetQueueItemView(QueueItemViewModel queueItemView, string id);
        
        public List<BinaryObject> AttachFiles(List<IFormFile> files, Guid queueItemId, QueueItem queueItem);
        
        public List<BinaryObject> UpdateAttachedFiles(UpdateQueueItemViewModel request, QueueItem queueItem);
    }
}