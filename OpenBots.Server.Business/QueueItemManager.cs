using Newtonsoft.Json;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public class QueueItemManager : BaseManager, IQueueItemManager
    {
        private readonly IQueueItemRepository repo;
        private readonly IQueueRepository queueRepository;
        private readonly IQueueItemAttachmentRepository queueItemAttachmentRepository;

        public QueueItemManager(IQueueItemRepository repo, IQueueRepository queueRepository, IQueueItemAttachmentRepository queueItemAttachmentRepository)
        {
            this.repo = repo;
            this.queueRepository = queueRepository;
            this.queueItemAttachmentRepository = queueItemAttachmentRepository;
        }

        public async Task<QueueItemModel> Enqueue(QueueItemModel item)
        {
            item.State = QueueItemStateType.New.ToString();
            item.StateMessage = "Successfully created new queue item.";
            item.IsLocked = false;
            if (item.Priority == 0)
                item.Priority = 100;

            return item;
        }

        public async Task<QueueItemModel> Dequeue(string agentId, string queueId)
        {
            string newState = QueueItemStateType.New.ToString();
            string inProgressState = QueueItemStateType.InProgress.ToString();

            var expiredItem = FindExpiredQueueItem(newState, inProgressState, queueId);
            while (expiredItem != null)
            {
                SetExpiredState(expiredItem);
                repo.Update(expiredItem);

                expiredItem = FindExpiredQueueItem(newState, inProgressState, queueId);
                if (expiredItem == null)
                    break;
            }

            var item = FindQueueItem(newState, queueId);
            if (item != null)
            {
                item.IsLocked = true;
                item.LockedOnUTC = DateTime.UtcNow;
                item.LockedUntilUTC = DateTime.UtcNow.AddHours(1);
                item.State = QueueItemStateType.InProgress.ToString();
                item.StateMessage = null;
                item.LockedBy = Guid.Parse(agentId);
                item.LockTransactionKey = Guid.NewGuid();
                repo.Update(item);
            }
            return item;
        }

        public QueueItemModel FindQueueItem(string state, string queueId)
        {
            var item = repo.Find(0, 1).Items
                .Where(q => q.QueueId.ToString() == queueId)
                .Where(q => q.State == state)
                .Where(q => !q.IsLocked)
                .Where(q => q.IsDeleted == false)
                .Where(q => q.PostponeUntilUTC <= DateTime.UtcNow || q.PostponeUntilUTC == null)
                .OrderByDescending(q => q.Priority)
                .ThenBy(q => q.CreatedOn)
                .FirstOrDefault();

            return item;
        }

        public QueueItemModel FindExpiredQueueItem(string newState, string inProgressState, string queueId)
        {
            var item = repo.Find(0, 1).Items
                .Where(q => q.QueueId.ToString() == queueId)
                .Where(q => q.State == newState || q.State == inProgressState)
                .Where(q => !q.IsLocked)
                .Where(q => q.IsDeleted == false)
                .Where(q => q.ExpireOnUTC <= DateTime.UtcNow)
                .FirstOrDefault();

            return item;
        }

        public async Task<QueueItemModel> Commit(Guid queueItemId, Guid transactionKey, string resultJSON)
        {
            var item = repo.GetOne(queueItemId);
            if (item.LockedUntilUTC <= DateTime.UtcNow)
            {
                SetNewState(item);
                repo.Update(item);
                return item;
            }
            else if (item?.IsLocked == true && item?.LockTransactionKey == transactionKey && item?.LockedUntilUTC >= DateTime.UtcNow)
            {
                item.ResultJSON = resultJSON;
                item.IsLocked = false;
                item.LockedUntilUTC = null;
                item.LockedEndTimeUTC = DateTime.UtcNow;
                item.LockTransactionKey = null;
                item.LockedBy = null;
                item.State = QueueItemStateType.Success.ToString();
                item.StateMessage = "Queue item transaction has been completed successfully";

                repo.Update(item);
                return item;
            }
            else
                throw new Exception("Transaction Key Mismatched or Expired. Cannot Commit.");
        }

        public async Task<QueueItemModel> Rollback(Guid queueItemId, Guid transactionKey, int retryLimit, string errorCode = null, string errorMessage = null, bool isFatal = false)
        {
            var item = repo.GetOne(queueItemId);
            if (item?.LockedUntilUTC < DateTime.UtcNow)
            {
                SetNewState(item);
                item.ErrorCode = errorCode;
                item.ErrorMessage = errorMessage;
                Dictionary<string, string> error = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(errorCode))
                    error.Add(errorCode, errorMessage);
                item.ErrorSerialized = JsonConvert.SerializeObject(error);

                repo.Update(item);
                return item;
            }
            else if (item?.IsLocked == true && item?.LockedUntilUTC >= DateTime.UtcNow && item?.LockTransactionKey == transactionKey)
            {
                item.IsLocked = false;
                item.LockTransactionKey = null;
                item.LockedEndTimeUTC = DateTime.UtcNow;
                item.LockedBy = null;
                item.LockedUntilUTC = null;
                item.RetryCount += 1;

                item.ErrorCode = errorCode;
                item.ErrorMessage = errorMessage;
                Dictionary<string, string> error = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(errorCode))
                    error.Add(errorCode, errorMessage);
                item.ErrorSerialized = JsonConvert.SerializeObject(error);

                if (isFatal)
                {
                    item.State = QueueItemStateType.Failed.ToString();
                    item.StateMessage = $"Queue item transaction {item.Name} has failed fatally.";
                }
                else
                {
                    if (item.RetryCount < retryLimit)
                    {
                        item.State = QueueItemStateType.New.ToString();
                        item.StateMessage = $"Queue item transaction {item.Name} failed {item.RetryCount} time(s).  Adding back to queue and trying again.";
                    }
                    else
                    {
                        item.State = QueueItemStateType.Failed.ToString();
                        item.StateMessage = $"Queue item transaction {item.Name} failed fatally and was unable to be automated {retryLimit} times.";
                    }
                }
                repo.Update(item);
                return item;
            }
            else
            {
                throw new Exception("Transaction key mismatched or expired. Cannot rollback.");
            }
        }

        public async Task<QueueItemModel> Extend(Guid queueItemId, Guid transactionKey, int extendByMinutes = 60)
        {
            var item = repo.GetOne(queueItemId);

            if (item?.LockedUntilUTC <= DateTime.UtcNow)
            {
                SetNewState(item);
                repo.Update(item);
                return item;
            }
            else if (item?.IsLocked == true && item?.LockTransactionKey == transactionKey && item?.LockedUntilUTC >= DateTime.UtcNow)
            {
                item.LockedUntilUTC = ((DateTime)item.LockedUntilUTC).AddMinutes(extendByMinutes);
                repo.Update(item);
                return item;
            }
            else
                throw new Exception("Transaction key mismatched or expired. Cannot extend.");
        }

        public async Task<QueueItemModel> UpdateState(Guid queueItemId, Guid transactionKey, string state = null, string stateMessage = null, string errorCode = null, string errorMessage = null)
        {
            var item = repo.GetOne(queueItemId);

            if (item?.LockedUntilUTC <= DateTime.UtcNow)
            {
                SetNewState(item);
                repo.Update(item);
                return item;
            }
            else if (item?.IsLocked == true && item?.LockTransactionKey == transactionKey && item?.LockedUntilUTC >= DateTime.UtcNow)
            {
                if (!string.IsNullOrEmpty(state))
                    item.State = state;
                if (!string.IsNullOrEmpty(stateMessage))
                    item.StateMessage = stateMessage;
                item.ErrorCode = errorCode;
                item.ErrorMessage = errorMessage;
                Dictionary<string, string> error = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(errorCode))
                    error.Add(errorCode, errorMessage);
                item.ErrorSerialized = JsonConvert.SerializeObject(error);
                repo.Update(item);
                return item;
            }
            else
                throw new Exception("Transaction key mismatched or expired.  Cannot update state.");
        }

        public async Task<QueueItemModel> GetQueueItem(Guid transactionKeyId)
        {
            QueueItemModel queueItem = repo.Find(0, 1).Items
                .Where(q => q.LockTransactionKey == transactionKeyId)
                .FirstOrDefault();

            return queueItem;
        }

        public enum QueueItemStateType : int
        {
            New = 0,
            InProgress = 1,
            Failed = 2,
            Success = 3,
            Expired = 4
        }

        public void SetNewState(QueueItemModel item)
        {
            item.RetryCount += 1;
            Guid queueId = item.QueueId;
            Queue queue = queueRepository.GetOne(queueId);
            int retryLimit = queue.MaxRetryCount;

            if (item.RetryCount < retryLimit)
            {
                item.State = QueueItemStateType.New.ToString();
                item.StateMessage = $"Queue item {item.Name}'s lock time has expired and failed {item.RetryCount} time(s).  Adding back to queue and trying again.";
            }
            else
            {
                item.State = QueueItemStateType.Failed.ToString();
                item.StateMessage = $"Queue item transaction {item.Name} failed fatally and was unable to be automated {retryLimit} times.";
            }
            item.IsLocked = false;
            item.LockedBy = null;
            item.LockedEndTimeUTC = null;
            item.LockedUntilUTC = null;
            item.LockTransactionKey = null;
        }

        public void SetExpiredState(QueueItemModel item)
        {
            item.State = QueueItemStateType.Expired.ToString();
            item.StateMessage = "Queue item has expired.";
            item.IsLocked = false;
            item.LockedBy = null;
            item.LockedEndTimeUTC = null;
            item.LockedUntilUTC = null;
            item.LockTransactionKey = null;
        }

        public PaginatedList<AllQueueItemsViewModel> GetQueueItemsAndBinaryObjectIds(Predicate<AllQueueItemsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }

        public QueueItemViewModel GetQueueItemView(QueueItemViewModel queueItemView, string id)
        {
            var attachmentsList = queueItemAttachmentRepository.Find(null, q => q.QueueItemId == Guid.Parse(id))?.Items;
            if (attachmentsList != null)
            {
                List<Guid> binaryObjectIds = new List<Guid>();
                foreach (var item in attachmentsList)
                {
                    binaryObjectIds.Add(item.BinaryObjectId);
                }
                queueItemView.BinaryObjectIds = binaryObjectIds;
            }
            else queueItemView.BinaryObjectIds = null;

            return queueItemView;
        }
    }
}
