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
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.File;
using OpenBots.Server.ViewModel.Queue;
using OpenBots.Server.ViewModel.QueueItem;
using OpenBots.Server.Web.Hubs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IOFile = System.IO.File;

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
        private readonly IStorageDriveRepository _storageDriveRepository;
        private readonly IStorageFileRepository _storageFileRepository;
        public IConfiguration Configuration { get; }

        public QueueItemManager(
            IQueueItemRepository repo,
            IQueueRepository queueRepository,
            IQueueItemAttachmentRepository queueItemAttachmentRepository,
            IHttpContextAccessor httpContextAccessor,
            IFileManager fileManager,
            IScheduleRepository schedulerepository,
            IHubManager hubManager,
            IConfiguration configuration,
            IStorageDriveRepository storageDriveRepository,
            IStorageFileRepository storageFileRepository)
        {
            _repo = repo;
            _queueRepository = queueRepository;
            _queueItemAttachmentRepository = queueItemAttachmentRepository;
            _httpContextAccessor = httpContextAccessor;
            _fileManager = fileManager;
            _scheduleRepo = schedulerepository;
            _hubManager = hubManager;
            Configuration = configuration;
            _storageDriveRepository = storageDriveRepository;
            _storageFileRepository = storageFileRepository;
        }

        public QueueItem Enqueue(QueueItem item)
        {
            item.State = QueueItemStateType.New.ToString();
            item.PayloadSizeInBytes = 0;
            item.StateMessage = "Successfully created new queue item.";
            item.IsLocked = false;
            if (item.Priority == 0)
                item.Priority = 100;

            //check if a queue arrival schedule exists for this queue
            var existingSchedules = _scheduleRepo.Find(null, s => s.QueueId == item.QueueId)?.Items;
            foreach (Schedule existingSchedule in existingSchedules)
            {
                if (existingSchedule != null && existingSchedule.IsDisabled == false && existingSchedule.StartingType.ToLower().Equals("queuearrival"))
                {
                    Schedule schedule = new Schedule();
                    schedule.Id = existingSchedule.Id;
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
                    schedule.MaxRunningJobs = existingSchedule.MaxRunningJobs;

                    var jsonScheduleObj = System.Text.Json.JsonSerializer.Serialize(schedule);
                    //call GetScheduleParameters()
                    var jobId = BackgroundJob.Enqueue(() => _hubManager.ExecuteJob(jsonScheduleObj, Enumerable.Empty<ParametersViewModel>()));
                }
            }         
            UpdateExpiredItemsStates(item.QueueId.ToString());

            return item;
        }

        public async Task<QueueItem> Dequeue(string agentId, string queueId)
        {
            string newState = QueueItemStateType.New.ToString();
            string inProgressState = QueueItemStateType.InProgress.ToString();

            UpdateExpiredItemsStates(queueId);

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

        public void UpdateExpiredItemsStates(string queueId)
        {
            string newState = QueueItemStateType.New.ToString();
            string inProgressState = QueueItemStateType.InProgress.ToString();

            var expiredItems = _repo.Find(0, 1).Items
                .Where(q => q.QueueId.ToString() == queueId)
                .Where(q => q.State == newState || q.State == inProgressState)
                .Where(q => !q.IsLocked)
                .Where(q => q.IsDeleted == false)
                .Where(q => q.ExpireOnUTC <= DateTime.UtcNow);

            foreach (var item in expiredItems)
            {
                item.State = QueueItemStateType.Expired.ToString();
                item.StateMessage = "Queue item has expired.";
                item.IsLocked = false;
                item.LockedBy = null;
                item.LockedEndTimeUTC = null;
                item.LockedUntilUTC = null;
                item.LockTransactionKey = null;
                _repo.Update(item);
            }
        }

        public async Task<QueueItem> Commit(Guid transactionKey, string resultJSON)
        {
            QueueItem queueItem = await GetQueueItem(transactionKey);

            if (queueItem == null) throw new EntityDoesNotExistException("Transaction key cannot be found");

            var item = _repo.GetOne(queueItem.Id.Value);
            if (item == null)
            {
                throw new EntityDoesNotExistException("QueueItem does not exist or you do not have authorized access.");
            }
            UpdateExpiredItemsStates(item.QueueId.ToString());

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
            if (item == null)
            {
                throw new EntityDoesNotExistException("QueueItem does not exist or you do not have authorized access.");
            }
            UpdateExpiredItemsStates(item.QueueId.ToString());

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
            if (item == null)
            {
                throw new EntityDoesNotExistException("QueueItem does not exist or you do not have authorized access.");
            }
            UpdateExpiredItemsStates(item.QueueId.ToString());

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
            if (item == null)
            {
                throw new EntityDoesNotExistException("QueueItem does not exist or you do not have authorized access.");
            }
            UpdateExpiredItemsStates(item.QueueId.ToString());

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

        public PaginatedList<AllQueueItemsViewModel> GetQueueItemsAndFileIds(Predicate<AllQueueItemsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
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
                queueItemViewModel.Attachments = attachmentsList;
            }
            else queueItemViewModel.Attachments = null;

            return queueItemViewModel;
        }

        public QueueItemViewModel UpdateAttachedFiles(QueueItem queueItem, UpdateQueueItemViewModel request)
        {
            if (queueItem == null) throw new EntityDoesNotExistException("Queue item could not be found or does not exist");
            UpdateExpiredItemsStates(queueItem.QueueId.ToString());

            queueItem.DataJson = request.DataJson;
            queueItem.Event = request.Event;
            queueItem.ExpireOnUTC = request.ExpireOnUTC;
            queueItem.PostponeUntilUTC = request.PostponeUntilUTC;
            queueItem.Name = request.Name;
            queueItem.QueueId = request.QueueId.Value;
            queueItem.Source = request.Source;
            queueItem.Type = request.Type;
            queueItem.State = request.State;

            if (queueItem.State == "New")
            {
                queueItem.StateMessage = null;
                queueItem.RetryCount = 0;
            }

            //if files don't exist in file manager: add file entity, upload file, and add email attachment attachment entity
            var attachments = _queueItemAttachmentRepository.Find(null, q => q.QueueItemId == queueItem.Id)?.Items;
            string hash = string.Empty;

            if (string.IsNullOrEmpty(request.DriveId))
            {
                var drive = new StorageDrive();
                if (attachments.Count() > 0)
                {
                    var fileToCheck = _storageFileRepository.GetOne(attachments[0].Id.Value);
                    drive = _storageDriveRepository.GetOne(fileToCheck.StorageDriveId.Value);
                }
                else
                    drive = _storageDriveRepository.Find(null, q => q.IsDefault == true).Items?.FirstOrDefault();

                request.DriveId = drive.Id.ToString();
            }

            IFormFile[] filesArray = CheckFiles(request.Files, hash, attachments, request.DriveId);
            var queueItemAttachments = new List<QueueItemAttachment>();
            if (filesArray.Length > 0)
                queueItemAttachments = AddNewAttachments(queueItem, filesArray, request.DriveId);

            _repo.Update(queueItem);

            //attach new files
            QueueItemViewModel response = new QueueItemViewModel();
            response = response.Map(queueItem);
            foreach (var attachment in attachments)
                queueItemAttachments.Add(attachment);
            response.Attachments = queueItemAttachments;

            return response;
        }

        public IFormFile[] CheckFiles(IFormFile[] files, string hash, List<QueueItemAttachment> attachments, string driveId)
        {
            if (files != null)
            {
                var filesList = files.ToList();

                if (string.IsNullOrEmpty(driveId))
                {
                    var fileToCheck = _storageFileRepository.GetOne(attachments[0].FileId);
                    var drive = _storageDriveRepository.GetOne(fileToCheck.StorageDriveId.Value);
                    driveId = drive.Id.ToString();
                }

                foreach (var attachment in attachments)
                {
                    var fileView = _fileManager.GetFileFolder(attachment.FileId.ToString(), driveId, "Files");
                    var originalHash = fileView.Hash;

                    //check if file with same hash and email id already exists
                    foreach (var file in files)
                    {
                        hash = GetHash(hash, file);

                        //if email attachment already exists and hash is the same: remove from files list
                        if (fileView.ContentType == file.ContentType && originalHash == hash && fileView.Size == file.Length)
                            filesList.Remove(file);

                        //if email attachment exists but the hash is not the same: update the attachment and file, remove from files list
                        else if (fileView.ContentType == file.ContentType && fileView.Name == file.FileName)
                        {
                            fileView = new FileFolderViewModel()
                            {
                                ContentType = file.ContentType,
                                Files = new IFormFile[] { file },
                                IsFile = true,
                                StoragePath = fileView.StoragePath,
                                Name = file.FileName,
                                Id = fileView.Id
                            };
                            attachment.SizeInBytes = file.Length;
                            _queueItemAttachmentRepository.Update(attachment);
                            _fileManager.UpdateFile(fileView);
                            filesList.Remove(file);
                        }
                    }
                }
                //if file doesn't exist, keep it in files list and return files to be attached
                var filesArray = filesList.ToArray();
                return filesArray;
            }
            else
                return Array.Empty<IFormFile>();
        }

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

        public List<QueueItemAttachment> AddFileAttachments(QueueItem queueItem, string[] requests, string driveId)
        {
            if (requests.Length == 0 || requests == null) throw new EntityOperationException("No files found to attach");

            var drive = GetDrive(driveId);
            driveId = drive.Id.ToString();

            long? payload = 0;
            var queueItemAttachments = new List<QueueItemAttachment>();
            var files = new List<FileFolderViewModel>();

            foreach (var request in requests)
            {
                var file = _fileManager.GetFileFolder(request, driveId, "Files");
                var fileToAdd = _storageFileRepository.GetOne(file.Id.Value);
                if (file == null) throw new EntityDoesNotExistException($"File could not be found");

                long? size = file.Size;
                if (size <= 0) throw new EntityOperationException($"File size of file {file.Name} cannot be 0");

                //create queue item attachment file under queue item id folder
                var path = Path.Combine(drive.Name, "Queue Item Attachments", queueItem.Id.ToString());
                using (var stream = IOFile.OpenRead(fileToAdd.StorageLocation))
                {
                    file.Files = new IFormFile[] { new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name)) };
                    file.StoragePath = path;
                    file.FullStoragePath = path;

                    CheckStoragePathExists(file, 0, queueItem.Id, driveId, drive.Name);
                    file = _fileManager.AddFileFolder(file, driveId)[0];
                    files.Add(file);
                }

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

            _fileManager.AddBytesToFoldersAndDrive(files);

            //update queue item payload
            queueItem.PayloadSizeInBytes += payload.Value;
            _repo.Update(queueItem);

            return queueItemAttachments;
        }

        public List<QueueItemAttachment> AddNewAttachments(QueueItem queueItem, IFormFile[] files, string driveId)
        {
            if (files.Length == 0 || files == null) throw new EntityOperationException("No files found to attach");

            driveId = CheckDriveId(driveId);
            var drive = GetDrive(driveId);
            driveId = drive.Id.ToString();

            //add files to drive
            string storagePath = Path.Combine(drive.Name, "Queue Item Attachments", queueItem.Id.ToString());
            var fileView = new FileFolderViewModel()
            {
                StoragePath = storagePath,
                FullStoragePath = storagePath,
                Files = files,
                IsFile = true
            };

            long? size = 0;
            foreach (var file in files)
                size += file.Length;

            CheckStoragePathExists(fileView, size, queueItem.Id, driveId, drive.Name);
            var fileViewList = _fileManager.AddFileFolder(fileView, driveId);
            var queueItemAttachments = new List<QueueItemAttachment>();
            long payloadSizeInBytes = 0;

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

                payloadSizeInBytes += queueItemAttachment.SizeInBytes;
                queueItem.PayloadSizeInBytes += payloadSizeInBytes;
                _repo.Update(queueItem);
            }

            return queueItemAttachments;
        }

        public FileFolderViewModel CheckStoragePathExists(FileFolderViewModel view, long? size, Guid? id, string driveId, string driveName)
        {
            //check if storage path exists; if it doesn't exist, create folder
            var folder = _fileManager.GetFileFolderByStoragePath(view.FullStoragePath, driveName);
            if (folder.Name == null)
            {
                folder.Name = id.ToString();
                folder.StoragePath = Path.Combine(driveName, "Queue Item Attachments");
                folder.IsFile = false;
                folder.Size = size;
                folder = _fileManager.AddFileFolder(folder, driveId)[0];
            }
            return folder;
        }

        public QueueItemAttachment UpdateAttachment(QueueItem queueItem, string id, IFormFile file, string driveId)
        {
            Guid entityId = new Guid(id);
            var existingAttachment = _queueItemAttachmentRepository.GetOne(entityId);
            if (existingAttachment == null) throw new EntityOperationException("No file found to update");

            driveId = CheckDriveId(driveId);
            var drive = _fileManager.GetDriveById(driveId);

            //update queue item payload
            long? originalSize = existingAttachment.SizeInBytes;
            long? size = file.Length;
            queueItem.PayloadSizeInBytes += size.Value - originalSize.Value;
            _repo.Update(queueItem);

            var fileView = _fileManager.GetFileFolder(existingAttachment.FileId.ToString(), driveId, "Files");

            if (fileView == null) throw new EntityDoesNotExistException($"File could not be found");

            size = fileView.Size;
            if (size <= 0) throw new EntityOperationException($"File size of file {fileView.Name} cannot be 0");

            //update queue item attachment entity
            var storagePath = Path.Combine(drive.Name, "Queue Item Attachments", queueItem.Id.ToString(), file.FileName);
            existingAttachment.SizeInBytes = file.Length;
            _queueItemAttachmentRepository.Update(existingAttachment);

            //update file entity and file
            fileView.Files = new IFormFile[] { file };
            fileView.StoragePath = storagePath;
            var fileViewModel = _fileManager.GetFileFolder(existingAttachment.FileId.ToString(), driveId, "Files");
            fileView.FullStoragePath = fileViewModel.FullStoragePath;
            _fileManager.UpdateFile(fileView);

            return existingAttachment;
        }

        public void DeleteQueueItem(QueueItem existingQueueItem, string driveId)
        {
            if (existingQueueItem == null)
                throw new EntityDoesNotExistException("Queue item does not exist or you do not have authorized access.");

            UpdateExpiredItemsStates(existingQueueItem.QueueId.ToString());

            //soft delete each queue item attachment entity and file entity that correlates to the queue item
            var attachments = _queueItemAttachmentRepository.Find(null, q => q.QueueItemId == existingQueueItem.Id)?.Items;
            if (attachments.Count != 0)
            {
                var fileView = new FileFolderViewModel();

                if (string.IsNullOrEmpty(driveId))
                {
                    if (attachments.Count() > 0)
                    {
                        var fileToDelete = _storageFileRepository.GetOne(attachments[0].FileId);
                        driveId = fileToDelete.StorageDriveId.ToString();
                    }
                    else
                    {
                        var drive = _storageDriveRepository.Find(null, q => q.IsDefault == true).Items?.FirstOrDefault();
                        driveId = drive.Id.ToString();
                    }
                }

                foreach (var attachment in attachments)
                {
                    fileView = _fileManager.DeleteFileFolder(attachment.FileId.ToString(), driveId, "Files");
                    _queueItemAttachmentRepository.SoftDelete(attachment.Id.Value);
                }
                var folder = _fileManager.GetFileFolder(fileView.ParentId.ToString(), driveId, "Folders");
                if (!folder.HasChild.Value)
                    _fileManager.DeleteFileFolder(folder.Id.ToString(), driveId, "Folders");
                else _fileManager.RemoveBytesFromFoldersAndDrive(new List<FileFolderViewModel> { fileView });
            }
            else throw new EntityDoesNotExistException("No attachments found to delete");
        }

        public void DeleteAll(QueueItem queueItem, string driveId)
        {
            DeleteQueueItem(queueItem, driveId);

            //update queue item payload
            queueItem.PayloadSizeInBytes = 0;
            _repo.Update(queueItem);
        }

        public void DeleteOne(QueueItemAttachment attachment, QueueItem queueItem, string driveId)
        {
            if (attachment != null)
            {
                if (string.IsNullOrEmpty(driveId))
                {
                    var fileToDelete = _storageFileRepository.GetOne(attachment.FileId);
                    driveId = fileToDelete.StorageDriveId.ToString();
                }

                var fileView = _fileManager.DeleteFileFolder(attachment.FileId.ToString(), driveId, "Files");
                var folder = _fileManager.GetFileFolder(fileView.ParentId.ToString(), driveId, "Folders");
                if (!folder.HasChild.Value)
                    _fileManager.DeleteFileFolder(folder.Id.ToString(), driveId, "Folders");
                else _fileManager.RemoveBytesFromFoldersAndDrive(new List<FileFolderViewModel> { fileView });

                //update queue item payload
                queueItem.PayloadSizeInBytes -= attachment.SizeInBytes;
                _repo.Update(queueItem);
            }
            else throw new EntityDoesNotExistException("Attachment could not be found");
        }

        public async Task<FileFolderViewModel> Export(string id, string driveId)
        {
            Guid attachmentId;
            Guid.TryParse(id, out attachmentId);

            QueueItemAttachment attachment = _queueItemAttachmentRepository.GetOne(attachmentId);
            if (attachment == null || attachment.FileId == null || attachment.FileId == Guid.Empty)
                throw new EntityDoesNotExistException($"Queue item attachment with id {id} could not be found or doesn't exist");

            driveId = CheckDriveIdByFileId(attachment.FileId.ToString(), driveId);

            var response = await _fileManager.ExportFileFolder(attachment.FileId.ToString(), driveId);
            return response;
        }

        private StorageDrive GetDrive(string driveId)
        {
            var drive = _storageDriveRepository.GetOne(Guid.Parse(driveId));
            if (drive == null)
            {
                drive = _storageDriveRepository.Find(null, q => q.IsDefault == true).Items?.FirstOrDefault();

                if (drive == null)
                    throw new EntityDoesNotExistException("Default drive could not be found or does not exist");
            }
            return drive;
        }

        private string CheckDriveId(string driveId)
        {
            if (string.IsNullOrEmpty(driveId))
            {
                var drive = _storageDriveRepository.Find(null, q => q.IsDefault == true).Items?.FirstOrDefault();
                if (drive == null)
                    throw new EntityDoesNotExistException("Default drive could not be found or does not exist");
                else
                    driveId = drive.Id.ToString();
            }
            return driveId;
        }

        private string CheckDriveIdByFileId(string id, string driveId)
        {
            if (string.IsNullOrEmpty(driveId))
            {
                var fileToExport = _storageFileRepository.GetOne(Guid.Parse(id));
                driveId = fileToExport.StorageDriveId.ToString();
            }
            return driveId;
        }
    }
}