using Newtonsoft.Json;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public class QueueItemManager : BaseManager, IQueueItemManager
    {
        private readonly IQueueItemRepository repo;

        public QueueItemManager(IQueueItemRepository repo)
        {
            this.repo = repo;
        }

        public async Task<QueueItem> Enqueue(QueueItem item)
        {
            item.State = QueueItemStateType.New.ToString();
            item.StateMessage = "Successfully created new queue item.";
            item.IsLocked = false;
            if (item.Priority == 0)
                item.Priority = 100;

            return item;
        }

        public async Task<QueueItem> Dequeue(string agentId, string queueId)
        {
            string state = QueueItemStateType.New.ToString();

            var item = FindQueueItem(state, queueId);

            if (item != null)
            { 
                if (item.ExpireOnUTC != null)
                {
                    while (item.ExpireOnUTC <= DateTime.UtcNow)
                    {
                        item.State = QueueItemStateType.Failed.ToString();
                        item.StateMessage = $"Queue item {item.Name} is expired.  Moving to next available queue item.";
                        repo.Update(item);

                        item = FindQueueItem(state, queueId);
                        if (item == null)
                            break;
                    }
                }

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
            }
            return item;
        }

        public QueueItem FindQueueItem(string state, string queueId)
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

        public async Task Commit(Guid queueItemId, Guid transactionKey)
        {

            var item = repo.GetOne(queueItemId);
            if (item?.IsLocked == true && item?.LockTransactionKey == transactionKey && item?.LockedUntilUTC >= DateTime.UtcNow)
            {
                item.IsLocked = false;
                item.LockedUntilUTC = null;
                item.LockedEndTimeUTC = DateTime.UtcNow;
                item.LockTransactionKey = null;
                item.LockedBy = null;
                item.State = QueueItemStateType.Success.ToString();
                item.StateMessage = "Queue item transaction has been completed successfully";

                repo.Update(item);
            }
            else
                throw new Exception("Transaction Key Mismatched or Expired. Cannot Commit.");
        }

        public async Task Rollback(Guid queueItemId, Guid transactionKey, int retryLimit, string errorCode = null, string errorMessage = null, bool isFatal = false)
        {
            var item = repo.GetOne(queueItemId);
            if (item?.IsLocked == true && item?.LockedUntilUTC >= DateTime.UtcNow && item?.LockTransactionKey == transactionKey)
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

                    if (item.RetryCount >= retryLimit)
                    {
                        item.State = QueueItemStateType.Failed.ToString();
                        item.StateMessage = $"Queue item transaction {item.Name} failed fatally and was unable to be processed {retryLimit} times.";
                    }
                }

                repo.Update(item);
            }
            else
            {
                throw new Exception("Transaction key mismatched or expired. Cannot rollback.");
            }
        }

        public async Task Extend(Guid queueItemId, Guid transactionKey, int extendByMinutes = 60)
        {
            var item = repo.GetOne(queueItemId);
            if (item?.IsLocked == true && item?.LockTransactionKey == transactionKey && item?.LockedUntilUTC >= DateTime.UtcNow)
            {
                item.LockedUntilUTC = ((DateTime)item.LockedUntilUTC).AddMinutes(extendByMinutes);
                repo.Update(item);
            }
            else
                throw new Exception("Transaction key mismatched or expired. Cannot extend.");
        }

        public async Task UpdateState(Guid queueItemId, Guid transactionKey, string state = null, string stateMessage = null, string errorCode = null, string errorMessage = null)
        {
            var item = repo.GetOne(queueItemId);
            if (item?.IsLocked == true && item?.LockTransactionKey == transactionKey && item?.LockedUntilUTC >= DateTime.UtcNow)
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
            }
            else
                throw new Exception("Transaction key mismatched or expired.  Cannot update state.");
        }

        public async Task<QueueItem> GetQueueItem(Guid transactionKeyId)
        {
            QueueItem queueItem = repo.Find(0, 1).Items
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
    }
}
