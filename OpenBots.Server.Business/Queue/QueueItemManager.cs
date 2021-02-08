using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.Queue;
using OpenBots.Server.ViewModel.QueueItem;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public class QueueItemManager : BaseManager, IQueueItemManager
    {
        private readonly IQueueItemRepository _repo;
        private readonly IQueueRepository _queueRepository;
        private readonly IQueueItemAttachmentRepository _queueItemAttachmentRepository;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IBinaryObjectRepository _binaryObjectRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QueueItemManager(
            IQueueItemRepository repo,
            IQueueRepository queueRepository,
            IQueueItemAttachmentRepository queueItemAttachmentRepository,
            IBinaryObjectManager binaryObjectManager,
            IHttpContextAccessor httpContextAccessor,
            IBinaryObjectRepository binaryObjectRepository)
        {
            _repo = repo;
            _queueRepository = queueRepository;
            _queueItemAttachmentRepository = queueItemAttachmentRepository;
            _binaryObjectManager = binaryObjectManager;
            _httpContextAccessor = httpContextAccessor;
            _binaryObjectRepository = binaryObjectRepository;
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
            string newState = QueueItemStateType.New.ToString();
            string inProgressState = QueueItemStateType.InProgress.ToString();

            var expiredItem = FindExpiredQueueItem(newState, inProgressState, queueId);
            while (expiredItem != null)
            {
                SetExpiredState(expiredItem);
                _repo.Update(expiredItem);

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
                _repo.Update(item);
            }
            return item;
        }

        public QueueItem FindQueueItem(string state, string queueId)
        {
            var item = _repo.Find(0, 1).Items
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

        public QueueItem FindExpiredQueueItem(string newState, string inProgressState, string queueId)
        {
            var item = _repo.Find(0, 1).Items
                .Where(q => q.QueueId.ToString() == queueId)
                .Where(q => q.State == newState || q.State == inProgressState)
                .Where(q => !q.IsLocked)
                .Where(q => q.IsDeleted == false)
                .Where(q => q.ExpireOnUTC <= DateTime.UtcNow)
                .FirstOrDefault();

            return item;
        }

        public async Task<QueueItem> Commit(Guid queueItemId, Guid transactionKey, string resultJSON)
        {
            var item = _repo.GetOne(queueItemId);
            if (item.LockedUntilUTC <= DateTime.UtcNow)
            {
                SetNewState(item);
                _repo.Update(item);
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

                _repo.Update(item);
                return item;
            }
            else
                throw new Exception("Transaction Key Mismatched or Expired. Cannot Commit.");
        }

        public async Task<QueueItem> Rollback(Guid queueItemId, Guid transactionKey, int retryLimit, string errorCode = null, string errorMessage = null, bool isFatal = false)
        {
            var item = _repo.GetOne(queueItemId);
            if (item?.LockedUntilUTC < DateTime.UtcNow)
            {
                SetNewState(item);
                item.ErrorCode = errorCode;
                item.ErrorMessage = errorMessage;
                Dictionary<string, string> error = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(errorCode))
                    error.Add(errorCode, errorMessage);
                item.ErrorSerialized = JsonConvert.SerializeObject(error);

                _repo.Update(item);
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
                _repo.Update(item);
                return item;
            }
            else
            {
                throw new Exception("Transaction key mismatched or expired. Cannot rollback.");
            }
        }

        public async Task<QueueItem> Extend(Guid queueItemId, Guid transactionKey, int extendByMinutes = 60)
        {
            var item = _repo.GetOne(queueItemId);

            if (item?.LockedUntilUTC <= DateTime.UtcNow)
            {
                SetNewState(item);
                _repo.Update(item);
                return item;
            }
            else if (item?.IsLocked == true && item?.LockTransactionKey == transactionKey && item?.LockedUntilUTC >= DateTime.UtcNow)
            {
                item.LockedUntilUTC = ((DateTime)item.LockedUntilUTC).AddMinutes(extendByMinutes);
                _repo.Update(item);
                return item;
            }
            else
                throw new Exception("Transaction key mismatched or expired. Cannot extend.");
        }

        public async Task<QueueItem> UpdateState(Guid queueItemId, Guid transactionKey, string state = null, string stateMessage = null, string errorCode = null, string errorMessage = null)
        {
            var item = _repo.GetOne(queueItemId);

            if (item?.LockedUntilUTC <= DateTime.UtcNow)
            {
                SetNewState(item);
                _repo.Update(item);
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
                _repo.Update(item);
                return item;
            }
            else
                throw new Exception("Transaction key mismatched or expired.  Cannot update state.");
        }

        public async Task<QueueItem> GetQueueItem(Guid transactionKeyId)
        {
            QueueItem queueItem = _repo.Find(0, 1).Items
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

        public void SetNewState(QueueItem item)
        {
            item.RetryCount += 1;
            Guid queueId = item.QueueId;
            Queue queue = _queueRepository.GetOne(queueId);
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

        public void SetExpiredState(QueueItem item)
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
            return _repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }

        public PaginatedList<AllQueueItemAttachmentsViewModel> GetQueueItemAttachmentsAndNames(Guid queueItemId, Predicate<AllQueueItemAttachmentsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return _queueItemAttachmentRepository.FindAllView(queueItemId, predicate, sortColumn, direction, skip, take);
        }

        public QueueItemViewModel GetQueueItemView(QueueItemViewModel queueItemView, string id)
        {
            var attachmentsList = _queueItemAttachmentRepository.Find(null, q => q.QueueItemId == Guid.Parse(id))?.Items;
            if (attachmentsList != null)
            {
                List<Guid?> binaryObjectIds = new List<Guid?>();
                foreach (var item in attachmentsList)
                {
                    binaryObjectIds.Add(item.BinaryObjectId);
                }
                queueItemView.BinaryObjectIds = binaryObjectIds;
            }
            else queueItemView.BinaryObjectIds = null;

            return queueItemView;
        }

        public List<BinaryObject> AttachFiles(List<IFormFile> files, Guid queueItemId, QueueItem queueItem)
        {
            var binaryObjects = new List<BinaryObject>();
            long payload = 0;

            if (files.Count != 0 || files != null)
            {
                foreach (var file in files)
                {
                    if (file == null)
                    {
                        throw new FileNotFoundException("No file attached");
                    }

                    long size = file.Length;
                    if (size <= 0)
                    {
                        throw new InvalidDataException($"File size of file {file.FileName} cannot be 0");
                    }

                    string organizationId = _binaryObjectManager.GetOrganizationId();
                    string apiComponent = "QueueItemAPI";

                    //create binary object
                    BinaryObject binaryObject = new BinaryObject()
                    {
                        Name = file.FileName,
                        Folder = apiComponent,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                        CorrelationEntityId = queueItemId
                    };

                    string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObject.Id.ToString());

                    //upload file to the Server
                    _binaryObjectManager.Upload(file, organizationId, apiComponent, binaryObject.Id.ToString());
                    _binaryObjectManager.SaveEntity(file, filePath, binaryObject, apiComponent, organizationId);
                    _binaryObjectRepository.Add(binaryObject);

                    //create queue item attachment
                    QueueItemAttachment attachment = new QueueItemAttachment()
                    {
                        BinaryObjectId = (Guid)binaryObject.Id,
                        QueueItemId = queueItemId,
                        CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                        CreatedOn = DateTime.UtcNow,
                        SizeInBytes = file.Length
                    };
                    _queueItemAttachmentRepository.Add(attachment);
                    binaryObjects.Add(binaryObject);
                    payload += attachment.SizeInBytes;
                }
            }
            //update queue item payload
            queueItem.PayloadSizeInBytes += payload;
            _repo.Update(queueItem);

            return binaryObjects;
        }

        public List<BinaryObject> UpdateAttachedFiles(UpdateQueueItemViewModel request, QueueItem queueItem)
        {
            var attachments = _queueItemAttachmentRepository.Find(null, q => q.QueueItemId == request.Id)?.Items;
            var files = request.Files.ToList();

            foreach (var attachment in attachments)
            {
                var binaryObject = _binaryObjectRepository.GetOne(attachment.BinaryObjectId);

                //check if file with same hash and queue item id already exists
                foreach (var file in request.Files)
                {
                    byte[] bytes = Array.Empty<byte>();
                    using (var ms = new MemoryStream())
                    {
                        file.CopyToAsync(ms);
                        bytes = ms.ToArray();
                    }

                    string hash = string.Empty;
                    using (SHA256 sha256Hash = SHA256.Create())
                    {
                        hash = _binaryObjectManager.GetHash(sha256Hash, bytes);
                    }

                    if (binaryObject.HashCode == hash && binaryObject.CorrelationEntityId == request.Id)
                    {
                        files.Remove(file);
                    }
                }
            }
            //if file doesn't exist in binary objects (list of files): add binary object entity, upload file, and add queue item attachment entity
            var binaryObjects = AttachFiles(files, (Guid)queueItem.Id, queueItem);

            return binaryObjects;
        }
    }
}
