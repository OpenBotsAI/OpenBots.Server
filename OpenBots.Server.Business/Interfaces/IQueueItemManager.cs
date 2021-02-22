using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.File;
using OpenBots.Server.ViewModel.Queue;
using OpenBots.Server.ViewModel.QueueItem;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public interface IQueueItemManager : IManager
    {
        QueueItem Enqueue(QueueItem item);
        Task<QueueItem> Dequeue(string agentId, string queueId);
        Task<QueueItem> Commit(Guid transactionKey, string resultJSON);
        Task<QueueItem> Rollback(Guid transactionKey, string errorCode = null, string errorMessage = null, bool isFatal = false);
        Task<QueueItem> Extend(Guid transactionKey, int extendByMinutes = 60);
        Task<QueueItem> UpdateState(Guid transactionKey, string state = null, string stateMessage = null, string errorCode = null, string errorMessage = null);
        Task<QueueItem> GetQueueItem(Guid transactionKeyId);
        enum QueueItemStateType : int { };
        PaginatedList<AllQueueItemsViewModel> GetQueueItemsAndFileIds(Predicate<AllQueueItemsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        PaginatedList<AllQueueItemAttachmentsViewModel> GetQueueItemAttachmentsAndNames(Guid queueItemId, Predicate<AllQueueItemAttachmentsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        QueueItemViewModel GetQueueItemView(QueueItem queueItem);
        //List<FileFolderViewModel> AttachFiles(List<IFormFile> files, Guid queueItemId, QueueItem queueItem, string driveName);
        //QueueItemViewModel UpdateAttachedFiles(QueueItem queueItem, UpdateQueueItemViewModel request);
        List<QueueItemAttachment> AddFileAttachments(QueueItem queueItem, string[] requests, string driveName = null);
        List<QueueItemAttachment> AddNewAttachments(QueueItem queueItem, IFormFile[] files, string driveName = null);
        QueueItemAttachment UpdateAttachment(QueueItem queueItem, string id, IFormFile file, string driveName = null);
        void DeleteQueueItem(QueueItem existingQueueItem, string driveName);
        void DeleteAll(QueueItem queueItem, string driveName = null);
        void DeleteOne(QueueItemAttachment attachment, QueueItem queueItem, string driveName = null);
    }
}