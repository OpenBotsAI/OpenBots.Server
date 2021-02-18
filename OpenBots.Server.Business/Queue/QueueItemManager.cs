using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.File;
using OpenBots.Server.ViewModel.Queue;
using OpenBots.Server.Web.Hubs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public class QueueItemManager : BaseManager, IQueueItemManager
    {
        private readonly IQueueItemRepository _repo;
        private readonly IQueueRepository _queueRepository;
        private readonly IQueueItemAttachmentRepository _queueItemAttachmentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFileManager _fileManager;
        private readonly IScheduleRepository _scheduleRepo;
        private readonly IHubManager _hubManager;
        public IConfiguration Configuration { get; }

        public QueueItemManager(
            IQueueItemRepository repo,
            IQueueRepository queueRepository,
            IQueueItemAttachmentRepository queueItemAttachmentRepository,
            IHttpContextAccessor httpContextAccessor,
            IFileManager fileManager,
            IScheduleRepository schedulerepository,
            IHubManager hubManager,
            IConfiguration configuration)
        {
            _repo = repo;
            _queueRepository = queueRepository;
            _queueItemAttachmentRepository = queueItemAttachmentRepository;
            _httpContextAccessor = httpContextAccessor;
            _fileManager = fileManager;
            _scheduleRepo = schedulerepository;
            _hubManager = hubManager;
            Configuration = configuration;
        }

        public QueueItem Enqueue(QueueItem item)
        {
            item.State = QueueItemStateType.New.ToString();
            item.StateMessage = "Successfully created new queue item.";
            item.IsLocked = false;
            if (item.Priority == 0)
                item.Priority = 100;

            //check if a queue arrival schedule exists for this queue
            Schedule existingSchedule = _scheduleRepo.Find(0, 1).Items?.Where(s => s.QueueId == item.QueueId)?.FirstOrDefault();
            if (existingSchedule != null && existingSchedule.IsDisabled == false && existingSchedule.StartingType.ToLower().Equals("queuearrival"))
            {
                Schedule schedule = new Schedule();
                schedule.AgentId = existingSchedule.AgentId;
                schedule.CRONExpression = "";
                schedule.LastExecution = DateTime.UtcNow;
                schedule.NextExecution = DateTime.UtcNow;
                schedule.IsDisabled = false;
                schedule.ProjectId = null;
                schedule.StartingType = "QueueArrival";
                schedule.Status = "New";
                schedule.ExpiryDate = DateTime.UtcNow.AddDays(1);
                schedule.StartDate = DateTime.UtcNow;
                schedule.AutomationId = existingSchedule.AutomationId;

                var jsonScheduleObj = System.Text.Json.JsonSerializer.Serialize(schedule);
                //call GetScheduleParameters()
                var jobId = BackgroundJob.Enqueue(() => _hubManager.ExecuteJob(jsonScheduleObj, Enumerable.Empty<ParametersViewModel>()));
            }

            QueueItem queueItem = new QueueItem()
            {
                CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                DataJson = item.DataJson,
                Event = item.Event,
                ExpireOnUTC = item.ExpireOnUTC,
                IsLocked = item.IsLocked,
                JsonType = item.JsonType,
                Name = item.Name,
                PostponeUntilUTC = item.PostponeUntilUTC,
                Priority = item.Priority,
                QueueId = item.QueueId,
                RetryCount = item.RetryCount,
                Source = item.Source,
                State = item.State,
                StateMessage = item.StateMessage,
                Type = item.Type,
                PayloadSizeInBytes = 0
            };

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

        public async Task<QueueItem> Commit(Guid transactionKey, string resultJSON)
        {
            QueueItem queueItem = await GetQueueItem(transactionKey);

            if (queueItem == null) throw new EntityDoesNotExistException("Transaction key cannot be found");

            var item = _repo.GetOne(queueItem.Id.Value);

            if (item.State == "New") throw new EntityOperationException("Queue item lock time has expired; adding back to queue and trying again");

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

        public async Task<QueueItem> Rollback(Guid transactionKey, string errorCode = null, string errorMessage = null, bool isFatal = false)
        {
            QueueItem queueItem = await GetQueueItem(transactionKey);

            if (queueItem == null) throw new EntityDoesNotExistException("Transaction key cannot be found");

            Guid queueItemId = queueItem.Id.Value;
            Guid queueId = queueItem.QueueId;
            Queue queue = _queueRepository.GetOne(queueId);
            int retryLimit = queue.MaxRetryCount;

            if (retryLimit == null || retryLimit == 0)
                retryLimit = int.Parse(Configuration["Queue.Global:DefaultMaxRetryCount"]);

            var item = _repo.GetOne(queueItemId);
            if (item.State == "Failed") throw new EntityOperationException(item.StateMessage);

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

        public async Task<QueueItem> Extend(Guid transactionKey, int extendByMinutes = 60)
        {
            QueueItem queueItem = await GetQueueItem(transactionKey);

            if (queueItem == null) throw new EntityDoesNotExistException("Transaction key cannot be found");

            Guid queueItemId = (Guid)queueItem.Id;
            var item = _repo.GetOne(queueItemId);

            if (item.State == "New") throw new EntityOperationException("Queue item was not able to be extended; locked until time has passed, adding back to queue and trying again");

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

        public async Task<QueueItem> UpdateState(Guid transactionKey, string state = null, string stateMessage = null, string errorCode = null, string errorMessage = null)
        {
            QueueItem queueItem = await GetQueueItem(transactionKey);

            if (queueItem == null) throw new EntityDoesNotExistException("Transaction key cannot be found");

            Guid queueItemId = queueItem.Id.Value;
            var item = _repo.GetOne(queueItemId);

            if (item.State == "New")
                throw new EntityOperationException("Queue item state was not able to be updated; locked until time has passed, adding back to queue and trying again");
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

        public QueueItemViewModel GetQueueItemView(QueueItem queueItem)
        {
            QueueItemViewModel queueItemViewModel = new QueueItemViewModel();
            queueItemViewModel = queueItemViewModel.Map(queueItem);

            var attachmentsList = _queueItemAttachmentRepository.Find(null, q => q.QueueItemId == queueItem.Id)?.Items;
            if (attachmentsList != null)
            {
                List<Guid?> fileIds = new List<Guid?>();
                foreach (var item in attachmentsList)
                {
                    fileIds.Add(item.FileId);
                }
                queueItemViewModel.FileIds = fileIds;
            }
            else queueItemViewModel.FileIds = null;

            return queueItemViewModel;
        }

        public List<FileFolderViewModel> AttachFiles(List<IFormFile> files, Guid queueItemId, QueueItem queueItem, string driveName)
        {
            long payload = 0;
            var fileView = new FileFolderViewModel()
            {
                StoragePath = Path.Combine(driveName, "Queue Item Attachments"),
                Files = files.ToArray()
            };
            var fileViewList = _fileManager.AddFileFolder(fileView, driveName);

            if (files.Count != 0 || files != null)
            {
                foreach (var file in fileViewList)
                {
                    if (file == null) throw new FileNotFoundException("No file attached");

                    long size = file.Size.Value;
                    if (size <= 0) throw new InvalidDataException($"File size of file {file.Name} cannot be 0");

                    //create queue item attachment
                    QueueItemAttachment attachment = new QueueItemAttachment()
                    {
                        FileId = file.Id.Value,
                        QueueItemId = queueItemId,
                        CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                        CreatedOn = DateTime.UtcNow,
                        SizeInBytes = file.Size.Value
                    };
                    _queueItemAttachmentRepository.Add(attachment);
                    payload += attachment.SizeInBytes;
                }
            }
            //update queue item payload
            queueItem.PayloadSizeInBytes += payload;
            _repo.Update(queueItem);

            return fileViewList;
        }

        //public QueueItemViewModel UpdateAttachedFiles(QueueItem queueItem, UpdateQueueItemViewModel request)
        //{
        //    if (queueItem == null) throw new EntityDoesNotExistException("Queue item could not be found or does not exist");

        //    queueItem.DataJson = request.DataJson;
        //    queueItem.Event = request.Event;
        //    queueItem.ExpireOnUTC = request.ExpireOnUTC;
        //    queueItem.PostponeUntilUTC = request.PostponeUntilUTC;
        //    queueItem.Name = request.Name;
        //    queueItem.QueueId = request.QueueId.Value;
        //    queueItem.Source = request.Source;
        //    queueItem.Type = request.Type;
        //    queueItem.State = request.State;

        //    if (queueItem.State == "New")
        //    {
        //        queueItem.StateMessage = null;
        //        queueItem.RetryCount = 0;
        //    }

        //    var attachments = _queueItemAttachmentRepository.Find(null, q => q.QueueItemId == request.Id)?.Items;
        //    var files = request.Files.ToList();

        //    foreach (var attachment in attachments)
        //    {
        //        var fileView = _fileManager.GetFileFolder(attachment.FileId.ToString(), request.DriveName);

        //        //check if file with same hash and queue item id already exists
        //        foreach (var file in request.Files)
        //        {
        //            byte[] bytes = Array.Empty<byte>();
        //            using (var ms = new MemoryStream())
        //            {
        //                file.CopyToAsync(ms);
        //                bytes = ms.ToArray();
        //            }

        //            var hash = string.Empty;
        //            hash = GetHash(hash, file);
        //            var originalHash = string.Empty;
        //            using (var stream = IOFile.OpenRead(fileView.FullStoragePath))
        //            {
        //                IFormFile originalFile = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));
        //                originalHash = GetHash(hash, originalFile);
        //            }

        //            if (originalHash == hash)
        //            {
        //                files.Remove(file);
        //            }
        //        }
        //    }
        //    //if file doesn't exist in list of files: add file entity, upload file, and add queue item attachment entity
        //    var fileViewList = AttachFiles(files, queueItem.Id.Value, queueItem, request.DriveName);

        //    //attach new files
        //    QueueItemViewModel response = new QueueItemViewModel();
        //    response = response.Map(queueItem);
        //    var fileIds = new List<Guid?>();
        //    foreach (var file in fileViewList)
        //        fileIds.Add(file.Id);
        //    response.FileIds = fileIds;

        //    return response;
        //}

        public string GetHash(string hash, IFormFile file)
        {
            byte[] bytes = Array.Empty<byte>();
            using (var ms = new MemoryStream())
            {
                file.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            using (SHA256 sha256Hash = SHA256.Create())
            {
                HashAlgorithm hashAlgorithm = sha256Hash;
                byte[] data = hashAlgorithm.ComputeHash(bytes);
                var sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                    sBuilder.Append(data[i].ToString("x2"));
                hash = sBuilder.ToString();
            }
            return hash;
        }

        public List<QueueItemAttachment> AddFileAttachments(QueueItem queueItem, string[] requests, string driveName)
        {
            if (requests.Length == 0 || requests == null) throw new EntityOperationException("No files found to attach");
            
            long? payload = 0;
            var queueItemAttachments = new List<QueueItemAttachment>();

            foreach (var request in requests)
            {
                var file = _fileManager.GetFileFolder(request, driveName);
                if (file == null) throw new EntityDoesNotExistException($"File could not be found");

                long? size = file.Size;
                if (size <= 0) throw new EntityOperationException($"File size of file {file.Name} cannot be 0");

                //create queue item attachment
                QueueItemAttachment queueItemAttachment = new QueueItemAttachment()
                {
                    FileId = file.Id.Value,
                    QueueItemId = queueItem.Id.Value,
                    CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                    CreatedOn = DateTime.UtcNow,
                    SizeInBytes = file.Size.Value
                };
                _queueItemAttachmentRepository.Add(queueItemAttachment);
                payload += queueItemAttachment.SizeInBytes;
                queueItemAttachments.Add(queueItemAttachment);
            }

            //update queue item payload
            queueItem.PayloadSizeInBytes += payload.Value;
            _repo.Update(queueItem);

            return queueItemAttachments;
        }

        public List<QueueItemAttachment> AddNewAttachments(QueueItem queueItem, IFormFile[] files, string driveName)
        {
            if (files.Length == 0 || files == null) throw new EntityOperationException("No files found to attach");

            //add files to drive
            var fileView = new FileFolderViewModel()
            {
                StoragePath = Path.Combine(driveName, "Queue Item Attachments"),
                Files = files,
                IsFile = true
            };
            List<FileFolderViewModel> fileViewList = _fileManager.AddFileFolder(fileView, driveName);

            var queueItemAttachments = new List<QueueItemAttachment>();

            foreach (var file in fileViewList)
            {
                //create queue item attachment
                QueueItemAttachment queueItemAttachment = new QueueItemAttachment()
                {
                    FileId = file.Id.Value,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                    QueueItemId = queueItem.Id.Value,
                    SizeInBytes = file.Size.Value,
                };
                _queueItemAttachmentRepository.Add(queueItemAttachment);
                queueItemAttachments.Add(queueItemAttachment);
            }

            return queueItemAttachments;
        }

        public QueueItemAttachment UpdateAttachment(QueueItem queueItem, string id, IFormFile file, string driveName)
        {
            Guid entityId = new Guid(id);
            var existingAttachment = _queueItemAttachmentRepository.GetOne(entityId);
            if (existingAttachment == null) throw new EntityOperationException("No file found to update");

            //update queue item payload
            long? originalSize = existingAttachment.SizeInBytes;
            long? size = file.Length;
            queueItem.PayloadSizeInBytes += size.Value - originalSize.Value;
            _repo.Update(queueItem);

            var fileView = _fileManager.GetFileFolder(existingAttachment.FileId.ToString(), driveName);

            if (fileView == null) throw new EntityDoesNotExistException($"File could not be found");

            size = fileView.Size;
            if (size <= 0) throw new EntityOperationException($"File size of file {fileView.Name} cannot be 0");

            //update queue item attachment entity
            var storagePath = Path.Combine(driveName, "Queue Item Attachments", file.FileName);
            existingAttachment.SizeInBytes = file.Length;
            _queueItemAttachmentRepository.Update(existingAttachment);

            //update file entity and file
            fileView.Files = new IFormFile[] { file };
            fileView.StoragePath = storagePath;
            var fileViewModel = _fileManager.GetFileFolder(existingAttachment.FileId.ToString(), driveName);
            fileView.FullStoragePath = fileViewModel.FullStoragePath;
            _fileManager.UpdateFile(fileView);

            return existingAttachment;
        }

        public void DeleteQueueItem(QueueItem existingQueueItem, string driveName)
        {
            if (existingQueueItem == null) throw new EntityDoesNotExistException("Queue item cannot be found or does not exist");
            if (existingQueueItem.IsLocked) throw new EntityOperationException("Queue item is locked at this time and cannot be deleted");

            //soft delete each queue item attachment entity and binary object entity that correlates to the queue item
            var attachmentsList = _queueItemAttachmentRepository.Find(null, q => q.QueueItemId == existingQueueItem.Id)?.Items;
            foreach (var attachment in attachmentsList)
            {
                _fileManager.DeleteFileFolder(attachment.Id.ToString(), driveName);
                _queueItemAttachmentRepository.SoftDelete(attachment.Id.Value);
            }
        }

        public void DeleteAll(QueueItem queueItem, string driveName)
        {
            var attachments = _queueItemAttachmentRepository.Find(null, q => q.QueueItemId == queueItem.Id)?.Items;
            if (attachments.Count != 0)
            {
                var fileList = new List<FileFolderViewModel>();
                foreach (var attachment in attachments)
                {
                    var fileView = _fileManager.DeleteFileFolder(attachment.FileId.ToString(), driveName);
                    fileList.Add(fileView);
                    _queueItemAttachmentRepository.SoftDelete(attachment.Id.Value);
                }

                _fileManager.AddBytesToFoldersAndDrive(fileList);

                //update queue item payload
                queueItem.PayloadSizeInBytes = 0;
                _repo.Update(queueItem);
            }
            else throw new EntityDoesNotExistException("Attachments could not be found");
        }

        public void DeleteOne(QueueItemAttachment attachment, QueueItem queueItem, string driveName)
        {
            if (attachment != null)
            {
                var fileView = _fileManager.DeleteFileFolder(attachment.FileId.ToString(), driveName);
                _fileManager.AddBytesToFoldersAndDrive(new List<FileFolderViewModel> { fileView });

                //update queue item payload
                queueItem.PayloadSizeInBytes -= attachment.SizeInBytes;
                _repo.Update(queueItem);
            }
            else throw new EntityDoesNotExistException("Attachment could not be found");
        }
    }
}