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
using OpenBots.Server.ViewModel.File;
using OpenBots.Server.Web.Webhooks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using IOFile = System.IO.File;

namespace OpenBots.Server.Business.File
{
    public class LocalFileStorageAdapter : ILocalFileStorageAdapter
    {
        private readonly IStorageFileRepository _storageFileRepository;
        private readonly IStorageDriveOperationRepository _storageDriveOperationRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrganizationManager _organizationManager;
        private readonly IStorageFolderRepository _storageFolderRepository;
        private readonly IStorageDriveRepository _storageDriveRepository;
        private readonly IWebhookPublisher _webhookPublisher;
        private readonly IDirectoryManager _directoryManager;
        private readonly IAuditLogRepository _auditLogRepository;

        public IConfiguration Configuration { get; }

        public LocalFileStorageAdapter(
            IStorageFileRepository storageFileRepository,
            IStorageDriveOperationRepository storageDriveOperationRepository,
            IHttpContextAccessor httpContextAccessor,
            IOrganizationManager organizationManager,
            IStorageFolderRepository storageFolderRepository,
            IStorageDriveRepository storageDriveRepository,
            IConfiguration configuration,
            IWebhookPublisher webhookPublisher,
            IDirectoryManager directoryManager,
            IAuditLogRepository auditLogRepository)
        {
            _storageDriveOperationRepository = storageDriveOperationRepository;
            _storageFileRepository = storageFileRepository;
            _httpContextAccessor = httpContextAccessor;
            _organizationManager = organizationManager;
            _storageFolderRepository = storageFolderRepository;
            _storageDriveRepository = storageDriveRepository;
            _webhookPublisher = webhookPublisher;
            _directoryManager = directoryManager;
            _auditLogRepository = auditLogRepository;
            Configuration = configuration;
        }

        public PaginatedList<FileFolderViewModel> GetFilesFolders(string driveId, bool? isFile = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100, string path = null)
        {
            var filesFolders = new PaginatedList<FileFolderViewModel>();
            var files = new List<FileFolderViewModel>();
            Guid? driveIdGuid = Guid.Parse(driveId);

            if (isFile.Equals(true))
            {
                //get all files
                filesFolders = _storageFileRepository.FindAllView(driveIdGuid, predicate, sortColumn, direction, skip, take, path);
            }
            else if (isFile.Equals(false))
            {
                //get all folders
                filesFolders = _storageFolderRepository.FindAllView(driveIdGuid, predicate, sortColumn, direction, skip, take, path);
            }
            else
            {
                //get all folders and files
                filesFolders = _storageFolderRepository.FindAllFilesFoldersView(driveIdGuid, predicate, sortColumn, direction, skip, take, path);
            }

            return filesFolders;
        }

        public FileFolderViewModel GetFileFolderViewModel(string id, string driveId, string type)
        {
            Guid? driveIdGuid = Guid.Parse(driveId);
            var fileFolder = new FileFolderViewModel();

            if (type == "Files")
            {
                var file = _storageFileRepository.Find(null).Items?.Where(q => q.Id.ToString() == id && q.StorageDriveId == driveIdGuid).FirstOrDefault();
                if (file == null)
                    throw new EntityDoesNotExistException($"File with id {id} could not be found or does not exist");

                var storageFolder = _storageFolderRepository.Find(null).Items?.Where(q => q.Id == file.StorageFolderId).FirstOrDefault();
                Guid? folderId = Guid.Empty;
                string storagePath = string.Empty;
                if (storageFolder != null)
                {
                    folderId = storageFolder.Id;
                    storagePath = storageFolder.StoragePath;
                }
                else
                    storagePath = GetDriveById(file.StorageDriveId).Name;

                fileFolder = fileFolder.Map(file, storagePath);
            }
            else if (type == "Folders")
            {
                var folder = _storageFolderRepository.Find(null).Items?.Where(q => q.Id.ToString() == id && q.StorageDriveId == driveIdGuid).FirstOrDefault();
                if (folder == null)
                    throw new EntityDoesNotExistException($"Folder with id {id} could not be found or does not exist");

                var pathArray = folder.StoragePath.Split(Path.DirectorySeparatorChar);
                var shortPathArray = new string[pathArray.Length - 1];
                for (int i = 0; i < pathArray.Length - 1; i++)
                {
                    string folderName = pathArray[i];
                    shortPathArray.SetValue(folderName, i);
                }

                bool hasChild = CheckFolderHasChild(folder.Id);

                string shortPath = string.Join(Path.DirectorySeparatorChar, shortPathArray);
                fileFolder = fileFolder.Map(folder, shortPath, hasChild);
            }
            else
                throw new EntityOperationException("File or folder could not be found or does not exist");

            return fileFolder;
        }

        public FileFolderViewModel GetFileFolderByStoragePath(string storagePath, string driveName)
        {
            var fileView = new FileFolderViewModel();
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            var driveId = GetDriveId(driveName);
            var shortPath = GetShortPath(storagePath);
            var storageFile = _storageFileRepository.Find(null, q => q.StoragePath == storagePath && q.StorageDriveId == driveId).Items?.FirstOrDefault();
            if (storageFile == null)
            {
                var storageFolder = _storageFolderRepository.Find(null, q => q.StoragePath == storagePath && q.StorageDriveId == driveId).Items?.FirstOrDefault();
                if (storageFolder != null)
                {
                    bool hasChild = CheckFolderHasChild(storageFolder.Id);
                    fileView = fileView.Map(storageFolder, shortPath, hasChild);
                }
            }
            else
                fileView = fileView.Map(storageFile, shortPath);

            return fileView;
        }

        public Dictionary<Guid?, string> GetDriveNames(string adapterType)
        {
            var driveNames = new Dictionary<Guid?, string>();
            var drives = _storageDriveRepository.Find(null).Items.Where(q => q.FileStorageAdapterType == adapterType);

            if (drives == null)
                throw new EntityDoesNotExistException("No drives could be found");

            foreach (var drive in drives)
                driveNames.Add(drive.Id, drive.Name);

            return driveNames;
        }
        public StorageDrive GetDriveByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = "Files";
            var storageDrive = _storageDriveRepository.Find(null).Items?.Where(q => q.Name == name).FirstOrDefault();
            if (storageDrive == null)
                throw new EntityDoesNotExistException("Storage drive could not be found");
            return storageDrive;
        }

        public int? GetFileCount(string driveId)
        {
            var files = _storageFileRepository.Find(null).Items?.Where(q => q.StorageDriveId.ToString() == driveId);
            int? count = files.Count();
            return count;
        }

        public int? GetFolderCount(string driveId)
        {
            var folders = _storageFolderRepository.Find(null).Items?.Where(q => q.StorageDriveId.ToString() == driveId);
            int? count = folders.Count();
            return count;
        }

        public List<FileFolderViewModel> AddFileFolder(FileFolderViewModel request, string driveId)
        {
            var fileFolderList = new List<FileFolderViewModel>();
            var newFileFolder = new FileFolderViewModel();

            Guid? driveIdGuid = Guid.Parse(driveId);
            StorageDrive drive = GetDriveById(driveIdGuid);

            //check directory and component folders exist
            string organizationId = drive.OrganizationId.ToString();
            bool orgDirectoryExists = _directoryManager.Exists(organizationId);
            if (!orgDirectoryExists)
                _directoryManager.CreateDirectory(organizationId);
            bool driveDirectoryExists = _directoryManager.Exists(Path.Combine(organizationId, driveId));
            if (!driveDirectoryExists)
                _directoryManager.CreateDirectory(Path.Combine(organizationId, driveId));

            if (request.IsFile.Value)
            {
                foreach (var file in request.Files)
                {
                    if (file == null)
                        throw new EntityOperationException("No file uploaded");

                    long size = file.Length;
                    if (size <= 0)
                        throw new EntityOperationException($"File size of file {file.FileName} cannot be 0");

                    //add file
                    request.FullStoragePath = Path.Combine(request.StoragePath, file.FileName);
                    if (request.ContentType == null)
                        request.ContentType = file.ContentType;
                    newFileFolder = SaveFile(request, file, drive);
                    fileFolderList.Add(newFileFolder);
                }

                //add size in bytes to parent folders
                request.Size = 0;
                foreach (var file in request.Files)
                    request.Size += file.Length;
                long? filesSizeInBytes = request.Size;
                string path = request.StoragePath;

                //check if last folder is an agent folder; if it is, remove it from the path
                var pathArray = path.Split(Path.DirectorySeparatorChar);
                string lastFolder = pathArray[pathArray.Length - 1];
                if (Guid.TryParse(lastFolder, out Guid agentId))
                    path = GetShortPath(path);
                AddBytesToParentFolders(path, filesSizeInBytes);

                //add size in bytes to storage drive
                AddBytesToStorageDrive(drive, filesSizeInBytes);
            }
            else
            {
                //add folder
                string shortPath = request.StoragePath;
                string path = Path.Combine(shortPath, request.Name);
                request.FullStoragePath = path;
                var parentId = GetFolderId(shortPath, drive.Name);
                var id = Guid.NewGuid();
                ExistingFolderCheck(path);
                Guid? orgId = drive.OrganizationId;
                long? size = 0;
                if (request.Size != null)
                    size = request.Size;

                StorageFolder storageFolder = new StorageFolder()
                {
                    Id = id,
                    ParentFolderId = parentId,
                    CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                    CreatedOn = DateTime.UtcNow,
                    Name = request.Name,
                    SizeInBytes = size,
                    StorageDriveId = drive.Id,
                    StoragePath = path,
                    OrganizationId = orgId
                };

                bool folderDirectoryExists = CheckFolderExists(shortPath);
                if (folderDirectoryExists)
                {
                    //create directory and add storage folder
                    _storageFolderRepository.Add(storageFolder);
                    _webhookPublisher.PublishAsync("Files.NewFolderCreated", storageFolder.Id.ToString(), storageFolder.Name);

                    var hasChild = false;
                    newFileFolder = newFileFolder.Map(storageFolder, request.StoragePath, hasChild);
                    fileFolderList.Add(newFileFolder);
                }
                else
                    throw new DirectoryNotFoundException("Storage path could not be found");
            }
            return fileFolderList;
        }

        public FileFolderViewModel SaveFile(FileFolderViewModel request, IFormFile file, StorageDrive drive)
        {
            Guid? id = Guid.NewGuid();
            string shortPath = request.StoragePath;
            string path = request.FullStoragePath;
            Guid? organizationId = _organizationManager.GetDefaultOrganization().Id;
            string orgId = organizationId.ToString();
            ExistingFileCheck(path);
            string[] fileNameArray = file.FileName.Split(".");
            string ext = fileNameArray[1];
            var storageLocation = Path.Combine(orgId, drive.Id.ToString(), $"{id}.{ext}");
            bool locationExists = _directoryManager.Exists(storageLocation);
            if (locationExists) throw new EntityAlreadyExistsException($"File with id {id} already exists");

            //upload file to local server
            bool folderExists = CheckFolderExists(shortPath);
            if (!folderExists)
                throw new DirectoryNotFoundException("Storage path could not be found");

            if (file.Length <= 0 || file.Equals(null)) throw new Exception("No file exists");
            if (file.Length > 0)
            {
                using (var stream = new FileStream(storageLocation, FileMode.Create))
                    file.CopyTo(stream);

                ConvertToBinaryObject(storageLocation);
            }

            Guid? folderId = GetFolderId(shortPath, drive.Name);
            var hash = GetHash(storageLocation);
            Guid? driveId = drive.Id;

            //add file properties to storage file entity
            var storageFile = new StorageFile()
            {
                Id = id,
                ContentType = request.ContentType != null ? request.ContentType : file.ContentType,
                CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                HashCode = hash,
                Name = file.FileName,
                SizeInBytes = file.Length,
                StorageFolderId = folderId,
                StoragePath = path,
                StorageProvider = drive.FileStorageAdapterType,
                OrganizationId = organizationId,
                StorageDriveId = drive.Id,
                StorageLocation = storageLocation
            };
            _storageFileRepository.Add(storageFile);
            _webhookPublisher.PublishAsync("Files.NewFileCreated", storageFile.Id.ToString(), storageFile.Name);

            //add file attribute entities
            var attributes = new Dictionary<string, int>()
            {
                { StorageDriveOperations.StorageCount.ToString(), 1 },
                { StorageDriveOperations.RetrievalCount.ToString(), 0 },
                { StorageDriveOperations.AppendCount.ToString(), 0 }
            };

            List<StorageDriveOperation> storageDriveOperations = new List<StorageDriveOperation>();
            foreach (var attribute in attributes)
            {
                var storageDriveOperation = new StorageDriveOperation()
                {
                    StorageFileId = id,
                    AttributeValue = attribute.Value,
                    CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                    CreatedOn = DateTime.UtcNow,
                    DataType = attribute.Value.GetType().ToString(),
                    Name = attribute.Key,
                    OrganizationId = organizationId,
                    StorageDriveId = driveId
                };
                _storageDriveOperationRepository.Add(storageDriveOperation);
                storageDriveOperations.Add(storageDriveOperation);
            }

            var viewModel = new FileFolderViewModel();
            viewModel = viewModel.Map(storageFile, shortPath);
            return viewModel;
        }

        public async Task<FileFolderViewModel> UpdateFile(FileFolderViewModel request)
        {
            Guid entityId = request.Id.Value;
            var storageFile = _storageFileRepository.GetOne(entityId);
            if (storageFile == null) throw new EntityDoesNotExistException("File could not be found");
            if (storageFile.StorageDriveId != request.StorageDriveId)
                throw new EntityOperationException("Storage drive provided does not match existing file's storage drive");

            var file = request.Files[0];
            string path = request.StoragePath;
            if (string.IsNullOrEmpty(path))
                path = storageFile.StoragePath;
            Guid? organizationId = _organizationManager.GetDefaultOrganization().Id;

            long? size = storageFile.SizeInBytes;

            //update file attribute entities
            List<StorageDriveOperation> storageDriveOperations = new List<StorageDriveOperation>();
            var attributes = _storageDriveOperationRepository.Find(null, q => q.StorageFileId == entityId).Items;
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    if (attribute.Name == StorageDriveOperations.AppendCount.ToString() || attribute.Name == StorageDriveOperations.StorageCount.ToString())
                    {
                        attribute.AttributeValue += 1;
                        _storageDriveOperationRepository.Update(attribute);
                    }
                    storageDriveOperations.Add(attribute);
                }
            }
            else throw new EntityDoesNotExistException("File attribute entities could not be found for this file");

            //update file stored in server
            string shortPath = GetShortPath(path);
            bool folderExists = CheckFolderExists(shortPath);
            if (!folderExists)
                throw new DirectoryNotFoundException("Storage path could not be found");

            string storageLocation = storageFile.StorageLocation;

            if (file.Length > 0)
            {
                //delete old file path
                IOFile.Delete(storageLocation);

                string[] fileNameArray = file.FileName.Split(".");
                string ext = fileNameArray[1];
                string[] storageLocationArray = storageLocation.Split(".");
                storageLocation = $"{storageLocationArray[0]}.{ext}";

                var stream = new FileStream(storageLocation, FileMode.Create, FileAccess.ReadWrite);
                using (stream)
                {
                    file.CopyTo(stream);
                }

                ConvertToBinaryObject(storageLocation);
            }

            var hash = GetHash(storageLocation);

            //update storage file entity properties
            storageFile.ContentType = file.ContentType;
            storageFile.HashCode = hash;
            storageFile.Name = file.FileName;
            storageFile.OrganizationId = organizationId;
            storageFile.SizeInBytes = file.Length;
            storageFile.StoragePath = request.StoragePath ?? storageFile.StoragePath;
            storageFile.StorageLocation = storageLocation;
            storageFile.StorageDriveOperations = storageDriveOperations;

            _storageFileRepository.Update(storageFile);
            _webhookPublisher.PublishAsync("Files.FileUpdated", storageFile.Id.ToString(), storageFile.Name).Wait();

            //update size in bytes of storage folders
            var drive = GetDriveById(storageFile.StorageDriveId);
            if (storageFile.StorageFolderId != drive.Id)
                AddBytesToParentFolders(storageFile.StoragePath, storageFile.SizeInBytes);

            //update size in bytes in storage drive
            size = request.Files[0].Length - size;
            AddBytesToStorageDrive(drive, size);

            var storageFileView = new FileFolderViewModel();
            storageFileView = storageFileView.Map(storageFile, shortPath);

            return storageFileView;
        }

        public async Task<FileFolderViewModel> ExportFile(string id, string driveId)
        {
            Guid entityId = Guid.Parse(id);
            Guid? driveIdGuid = Guid.Parse(driveId);
            var file = _storageFileRepository.GetOne(entityId);

            if (file == null)
                throw new EntityDoesNotExistException("No file found to export");

            var fileFolder = new FileFolderViewModel();

            if (driveIdGuid != file.StorageDriveId) throw new EntityDoesNotExistException($"File {file.Name} does not exist in current drive with id {driveId}");

            var auditLog = new AuditLog()
            {
                ChangedFromJson = null,
                ChangedToJson = JsonConvert.SerializeObject(file),
                CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                ExceptionJson = "",
                ParametersJson = "",
                ObjectId = file.Id,
                MethodName = "Download",
                ServiceName = ToString()
            };

            _auditLogRepository.Add(auditLog);

            //export file
            var stream = new FileStream(file.StorageLocation, FileMode.Open, FileAccess.Read);
            fileFolder.StoragePath = file.StoragePath;
            fileFolder.Name = file.Name;
            fileFolder.ContentType = file.ContentType;
            fileFolder.Size = file.SizeInBytes;
            fileFolder.Id = file.Id;
            fileFolder.Content = stream;

            await stream.FlushAsync();

            //update file attribute: retrieval count
            var retrievalStorageDriveOperation = _storageDriveOperationRepository.Find(null).Items?.Where(q => q.StorageFileId == file.Id && q.Name == StorageDriveOperations.RetrievalCount.ToString()).FirstOrDefault();
            if (retrievalStorageDriveOperation != null)
            {
                retrievalStorageDriveOperation.AttributeValue += 1;
                _storageDriveOperationRepository.Update(retrievalStorageDriveOperation);
            }

            return fileFolder;
        }

        public FileFolderViewModel DeleteFileFolder(string id, string driveId, string type)
        {
            FileFolderViewModel fileFolder = new FileFolderViewModel();
            Guid? driveIdGuid = Guid.Parse(driveId);
            var drive = GetDriveById(driveIdGuid);
            
            if (type == "Files")
            {
                var storageFile = _storageFileRepository.Find(null).Items?.Where(q => q.Id.ToString() == id && q.StorageDriveId == driveIdGuid).FirstOrDefault();
                if (storageFile == null)
                    throw new EntityDoesNotExistException($"File with id {id} could not be found or does not exist");
                string shortPath = GetShortPath(storageFile.StoragePath);
                fileFolder = fileFolder.Map(storageFile, shortPath);
                DeleteFile(storageFile);
            }
            else if (type == "Folders")
            {
                var storageFolder = _storageFolderRepository.Find(null).Items?.Where(q => q.Id.ToString() == id && q.StorageDriveId == driveIdGuid).FirstOrDefault();
                if (storageFolder == null)
                    throw new EntityDoesNotExistException($"Folder with id {id} could not be found or does not exist");

                string shortPath = GetShortPath(storageFolder.StoragePath);
                bool hasChild = CheckFolderHasChild(storageFolder.Id);
                fileFolder = fileFolder.Map(storageFolder, shortPath, hasChild);
                if (storageFolder.SizeInBytes > 0 && !storageFolder.StoragePath.Contains("Queue Item Attachments") &&
                    !storageFolder.StoragePath.Contains("Automations") && !storageFolder.StoragePath.Contains("Assets") &&
                    !storageFolder.StoragePath.Contains("Email Attachments"))
                    throw new EntityOperationException("Folder cannot be deleted because it has files inside");
                else if (hasChild)
                    throw new EntityOperationException("Folder cannot be deleted because it has folders inside");
                else DeleteFolder(storageFolder);
            }
            else
                throw new EntityDoesNotExistException($"File or folder with id {id} could not be found or does not exist");

            return fileFolder;
        }

        public FileFolderViewModel RenameFileFolder(string id, string name, string driveId, string type)
        {
            Guid? entityId = Guid.Parse(id);
            var fileFolder = new FileFolderViewModel();
            Guid? driveIdGuid = Guid.Parse(driveId);

            if (type == "Files")
            {
                //rename file
                var storageFile = _storageFileRepository.Find(null).Items?.Where(q => q.Id == entityId && q.StorageDriveId == driveIdGuid).FirstOrDefault();
                if (storageFile == null)
                    throw new EntityDoesNotExistException($"File with id {id} could not be found or does not exist");

                string oldPath = storageFile.StoragePath;
                var newNameArray = storageFile.Name.Split(".");
                var contentType = newNameArray[newNameArray.Length - 1];
                string newName = name + "." + contentType;
                int index = oldPath.Split(Path.DirectorySeparatorChar).Length - 1;
                string newPath = GetNewPath(storageFile.StoragePath, storageFile.Name, newName, index);
                ExistingFileCheck(newPath);
                var newPathArray = newPath.Split(Path.DirectorySeparatorChar);
                var shortPathArray = new string[newPathArray.Length - 1];
                for (int i = 0; i < newPathArray.Length - 1; i++)
                {
                    string folderName = newPathArray[i];
                    shortPathArray.SetValue(folderName, i);
                }
                string shortPath = string.Join(Path.DirectorySeparatorChar, shortPathArray);

                storageFile.Name = newName;
                storageFile.StoragePath = newPath;
                storageFile.StorageLocation = storageFile.StorageLocation.Replace(storageFile.Name, newName);
                _storageFileRepository.Update(storageFile);

                _webhookPublisher.PublishAsync("Files.FileUpdated", id, storageFile.Name);

                fileFolder = fileFolder.Map(storageFile, shortPath);

                //update append file attribute
                var appendAttribute = _storageDriveOperationRepository.Find(null).Items?.Where(q => q.Name == StorageDriveOperations.AppendCount.ToString() && q.StorageFileId == storageFile.Id).FirstOrDefault();
                if (appendAttribute != null)
                    appendAttribute.AttributeValue += 1;
                _storageDriveOperationRepository.Update(appendAttribute);
            }
            else if (type == "Folders")
            {
                var storageFolder = _storageFolderRepository.Find(null).Items?.Where(q => q.Id == entityId && q.StorageDriveId == driveIdGuid).FirstOrDefault();
                if (storageFolder == null)
                    throw new EntityDoesNotExistException($"Folder with id {id} could not be found or does not exist");

                //rename folder
                bool hasChild = CheckFolderHasChild(entityId);
                string oldPath = storageFolder.StoragePath;
                string oldName = storageFolder.Name;
                int index = oldPath.Split(Path.DirectorySeparatorChar).Length - 1;
                string newPath = GetNewPath(storageFolder.StoragePath, oldName, name, index);
                ExistingFolderCheck(newPath);
                var newPathArray = newPath.Split(Path.DirectorySeparatorChar);
                string shortPath = GetShortPath(newPath);

                //move folder to new directory path
                storageFolder.Name = name;
                storageFolder.StoragePath = newPath;
                _storageFolderRepository.Update(storageFolder);
                _webhookPublisher.PublishAsync("Files.FolderUpdated", id, storageFolder.Name);

                //rename all files and folders underneath it
                if (hasChild)
                    RenameChildFilesFolders(entityId, oldName, name, index);
                fileFolder = fileFolder.Map(storageFolder, shortPath, hasChild);
            }
            else
                throw new EntityDoesNotExistException($"File or folder with id {id} could not be found or does not exist");

            return fileFolder;
        }
   
        public FileFolderViewModel MoveFileFolder(string fileFolderId, string parentFolderId, string driveId, string type)
        {
            Guid? entityId = Guid.Parse(fileFolderId);
            Guid? parentId = Guid.Parse(parentFolderId);
            var fileFolder = new FileFolderViewModel();
            Guid? driveIdGuid = Guid.Parse(driveId);
            var drive = GetDriveById(driveIdGuid);
            var parentFolder = _storageFolderRepository.GetOne(parentId.Value);
            string parentFolderPath;
            if (parentFolder != null)
                parentFolderPath = parentFolder.StoragePath;
            else
                parentFolderPath = drive.StoragePath;

            if (type == "Files")
            {
                //move file
                var storageFile = _storageFileRepository.Find(null).Items?.Where(q => q.Id == entityId && q.StorageDriveId == driveIdGuid).FirstOrDefault();
                if (storageFile == null)
                    throw new EntityDoesNotExistException($"File with id {fileFolderId} could not be found or does not exist");

                Guid? oldParentFolderId = storageFile.StorageFolderId;
                string oldPath = storageFile.StoragePath;
                string newPath = Path.Combine(parentFolderPath, storageFile.Name);
                ExistingFileCheck(newPath);

                storageFile.StoragePath = newPath;
                storageFile.StorageFolderId = parentId;
                _storageFileRepository.Update(storageFile);
                _webhookPublisher.PublishAsync("Files.FileUpdated", fileFolderId, storageFile.Name);

                fileFolder = fileFolder.Map(storageFile, parentFolderPath);

                //update append file attribute
                var appendAttribute = _storageDriveOperationRepository.Find(null, q => q.Name == StorageDriveOperations.AppendCount.ToString() && q.StorageFileId == storageFile.Id).Items?.FirstOrDefault();
                if (appendAttribute != null)
                    appendAttribute.AttributeValue += 1;
                _storageDriveOperationRepository.Update(appendAttribute);

                //update new and old parent folder sizes
                UpdateFolderSize(oldParentFolderId, parentFolderPath, storageFile);
            }
            else if (type == "Folders")
            {
                var storageFolder = _storageFolderRepository.Find(null).Items?.Where(q => q.Id == entityId && q.StorageDriveId == driveIdGuid).FirstOrDefault();           //else 
                if (storageFolder == null)
                    throw new EntityDoesNotExistException($"Folder with id {fileFolderId} could not be found or does not exist");

                //move folder
                bool hasChild = CheckFolderHasChild(entityId);
                Guid? oldParentFolderId = storageFolder.ParentFolderId;
                string oldPath = storageFolder.StoragePath;
                string newPath = Path.Combine(parentFolderPath, storageFolder.Name);
                int index = oldPath.Split(Path.DirectorySeparatorChar).Length - 1;
                ExistingFolderCheck(newPath);

                storageFolder.StoragePath = newPath;
                storageFolder.ParentFolderId = parentId;
                _storageFolderRepository.Update(storageFolder);
                _webhookPublisher.PublishAsync("Files.FolderUpdated", storageFolder.Id.ToString(), storageFolder.Name);

                //rename all files and folders underneath it
                if (hasChild)
                    RenameChildFilesFolders(entityId, oldPath, newPath, index);
                fileFolder = fileFolder.Map(storageFolder, parentFolderPath, hasChild);

                //update new and old parent folder sizes
                UpdateFolderSize(oldParentFolderId, parentFolderPath, null, storageFolder);
            }
            else
                throw new EntityDoesNotExistException($"File or folder with id {fileFolderId} could not be found or does not exist");

            return fileFolder;
        }

        public FileFolderViewModel CopyFileFolder(string fileFolderId, string parentFolderId, string driveId, string type)
        {
            Guid? entityId = Guid.Parse(fileFolderId);
            Guid? parentId = Guid.Parse(parentFolderId);
            var fileFolder = new FileFolderViewModel();
            Guid? driveIdGuid = Guid.Parse(driveId);
            var drive = GetDriveById(driveIdGuid);
            var parentFolder = _storageFolderRepository.GetOne(parentId.Value);
            string parentFolderPath;
            if (parentFolder != null)
                parentFolderPath = parentFolder.StoragePath;
            else
                parentFolderPath = drive.StoragePath;

            if (type == "Files")
            {
                //copy file
                var storageFile = _storageFileRepository.Find(null).Items?.Where(q => q.Id == entityId && q.StorageDriveId == driveIdGuid).FirstOrDefault();
                if (storageFile == null)
                    throw new EntityDoesNotExistException($"File with id {fileFolderId} could not be found or does not exist");

                string oldPath = storageFile.StoragePath;
                string newPath = Path.Combine(parentFolderPath, storageFile.Name);

                using (var stream = IOFile.OpenRead(storageFile.StorageLocation))
                {
                    IFormFile file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                    fileFolder = fileFolder.Map(storageFile, parentFolderPath);
                    fileFolder.StoragePath = parentFolderPath;
                    fileFolder.FullStoragePath = newPath;
                    fileFolder.UpdatedOn = null;
                    fileFolder.ParentId = parentId;
                    fileFolder.CreatedBy = null;
                    fileFolder.CreatedOn = null;
                    //create new file and file attributes in database
                    //create new file in storage drive
                    fileFolder = SaveFile(fileFolder, file, drive);
                    var newFile = _storageFileRepository.GetOne(fileFolder.Id.Value);

                    //add size in bytes of file to new parent folders and storage drive
                    UpdateFolderSize(storageFile.StorageFolderId, parentFolderId, newFile);

                    drive.StorageSizeInBytes += file.Length;
                    _storageDriveRepository.Update(drive);
                }
            }
            else if (type == "Folders")
            {
                var storageFolder = _storageFolderRepository.Find(null).Items?.Where(q => q.Id == entityId && q.StorageDriveId == driveIdGuid).FirstOrDefault();           //else 
                if (storageFolder == null)
                    throw new EntityDoesNotExistException($"Folder with id {fileFolderId} could not be found or does not exist");

                string oldPath = storageFolder.StoragePath;
                string newPath = Path.Combine(parentFolderPath, storageFolder.Name);
                ExistingFolderCheck(newPath);
                Guid? oldParentFolderId = storageFolder.Id;

                //create folder entity in database
                var storageFolderCopy = new StorageFolder();
                storageFolderCopy.ParentFolderId = parentId;
                storageFolderCopy.Id = Guid.NewGuid();
                storageFolderCopy.StoragePath = newPath;
                storageFolderCopy.CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name;
                storageFolderCopy.CreatedOn = DateTime.UtcNow;
                storageFolderCopy.Id = Guid.NewGuid();
                storageFolderCopy.StorageDriveId = storageFolder.StorageDriveId;
                storageFolderCopy.SizeInBytes = storageFolder.SizeInBytes;
                storageFolderCopy.OrganizationId = storageFolder.OrganizationId;
                storageFolderCopy.Name = storageFolder.Name;
                _storageFolderRepository.Add(storageFolderCopy);

                //map storage folder to file folder view model
                bool hasChild = CheckFolderHasChild(entityId);
                fileFolder = fileFolder.Map(storageFolderCopy, parentFolderPath, hasChild);

                if (hasChild)
                    CopyChildFilesFolders(storageFolder, oldPath, newPath, drive, oldParentFolderId, entityId, parentFolderPath);
            }
            else
                throw new EntityDoesNotExistException($"Folder or file with id {fileFolderId} could not be found or does not exist");

            return fileFolder;
        }

        public void AddBytesToFoldersAndDrive(List<FileFolderViewModel> files)
        {
            long? size = 0;
            var storagePath = files[0].FullStoragePath;
            var storageDriveId = files[0].StorageDriveId;
            foreach (var file in files)
                size += file.Size;

            //update size in bytes in folder
            AddBytesToParentFolders(storagePath, size);

            //update size in bytes in storage drive
            var drive = GetDriveById(storageDriveId);
            AddBytesToStorageDrive(drive, size);
        }

        public void RemoveBytesFromFoldersAndDrive(List<FileFolderViewModel> files)
        {
            long? size = 0;
            var storagePath = files[0].FullStoragePath;
            var storageDriveId = files[0].StorageDriveId;
            foreach (var file in files)
                size -= file.Size;

            //update size in bytes in folder
            AddBytesToParentFolders(storagePath, size);

            //update size in bytes in storage drive
            var drive = GetDriveById(storageDriveId);
            AddBytesToStorageDrive(drive, size);
        }

        public StorageDrive AddStorageDrive(string driveName)
        {
            Guid? organizationId = _organizationManager.GetDefaultOrganization().Id;

            var existingStorageDrive = _storageDriveRepository.Find(null, q => q.Name == driveName).Items?.FirstOrDefault();
            if (existingStorageDrive != null)
                throw new EntityAlreadyExistsException($"Drive with name {driveName} already exists");

            Guid? id = Guid.NewGuid();
            string storagePath = Path.Combine(organizationId.ToString(), id.ToString());
            var storageDrive = new StorageDrive()
            {
                Id = id,
                FileStorageAdapterType = "LocalFileStorage",
                CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                Name = driveName,
                OrganizationId = organizationId,
                StoragePath = storagePath,
                StorageSizeInBytes = 0,
                IsDefault = false,
            };
            _storageDriveRepository.Add(storageDrive);
            _directoryManager.CreateDirectory(storagePath);

            _webhookPublisher.PublishAsync("Files.NewDriveCreated", storageDrive.Id.ToString(), storageDrive.Name);

            return storageDrive;
        }

        public string GetShortPath(string path)
        {
            var newPathArray = path.Split(Path.DirectorySeparatorChar);
            var shortPathArray = new string[newPathArray.Length - 1];
            for (int i = 0; i < newPathArray.Length - 1; i++)
            {
                string folderName = newPathArray[i];
                shortPathArray.SetValue(folderName, i);
            }
            string shortPath = string.Join(Path.DirectorySeparatorChar, shortPathArray);
            return shortPath;
        }

        protected enum StorageDriveOperations
        {
            StorageCount,
            RetrievalCount,
            AppendCount
        }

        #region Private Methods

        private void AddBytesToStorageDrive(StorageDrive storageDrive, long? size)
        {
            //add to storage size in bytes property in storage drive
            storageDrive.StorageSizeInBytes += size;
            _storageDriveRepository.Update(storageDrive);
            _webhookPublisher.PublishAsync("Files.DriveUpdated", storageDrive.Id.ToString(), storageDrive.Name);
        }

        private void AddBytesToParentFolders(string path, long? size)
        {
            var pathArray = path.Split(Path.DirectorySeparatorChar);
            List<Guid?> parentIds = GetParentIds(pathArray);
            foreach (var storageFolderId in parentIds)
            {
                var folder = _storageFolderRepository.Find(null).Items?.Where(q => q.Id == storageFolderId).FirstOrDefault();
                if (folder != null)
                {
                    folder.SizeInBytes += size;
                    _storageFolderRepository.Update(folder);
                    _webhookPublisher.PublishAsync("Files.FolderUpdated", folder.Id.ToString(), folder.Name);
                }
            }
        }

        private long? DeleteFile(StorageFile storageFile)
        {
            //remove file attribute entities
            var attributes = _storageDriveOperationRepository.Find(null, q => q.StorageFileId == storageFile.Id).Items;
            if (attributes.Count() > 0)
            {
                foreach (var attribute in attributes)
                    _storageDriveOperationRepository.SoftDelete((Guid)attribute.Id);
            }

            //remove file
            IOFile.Delete(storageFile.StorageLocation);

            //remove storage file entity
            _storageFileRepository.SoftDelete(storageFile.Id.Value);
            _webhookPublisher.PublishAsync("Files.FileDeleted", storageFile.Id.ToString(), storageFile.Name);

            long? size = -storageFile.SizeInBytes;
            return size;
        }

        private Guid? GetDriveId(string driveName)
        {
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            StorageDrive drive = GetDriveByName(driveName);
            Guid? driveId;
            if (drive != null)
                driveId = drive.Id;
            else throw new EntityDoesNotExistException($"Drive {driveName} could not be found or does not exist");

            return driveId;
        }

        private bool CheckFolderExists(string path)
        {
            var folder = _storageFolderRepository.Find(null, q => q.StoragePath == path).Items?.FirstOrDefault();
            StorageDrive drive = null;

            if (folder == null)
                drive = _storageDriveRepository.Find(null, q => q.StoragePath == path).Items?.FirstOrDefault();
            
            if (folder != null || drive != null)
                return true;
            else
                return false;
        }

        private string GetHash(string path)
        {
            string hash = string.Empty;
            byte[] bytes = IOFile.ReadAllBytes(path);
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

        private void ConvertToBinaryObject(string filePath)
        {
            byte[] bytes = IOFile.ReadAllBytes(filePath);
            IOFile.WriteAllBytes(filePath, bytes);
        }

        private StorageFolder GetFolder(string path, string driveName)
        {
            Guid? driveId = GetDriveId(driveName);
            var storageFolder = _storageFolderRepository.Find(null).Items?.Where(q => q.StoragePath.ToLower() == path.ToLower() && q.StorageDriveId == driveId).FirstOrDefault();
            if (storageFolder == null)
                return null;
            return storageFolder;
        }

        private Guid? GetFolderId(string path, string driveName)
        {
            var folder = GetFolder(path, driveName);
            Guid? folderId = folder?.Id;
            if (folderId == null)
            {
                if (string.IsNullOrEmpty(driveName))
                    driveName = "Files";
                var storageDrive = GetDriveByName(driveName);
                if (storageDrive != null)
                    folderId = storageDrive.Id;
                else throw new EntityDoesNotExistException("Drive could not be found");
            }

            return folderId;
        }

        public StorageDrive GetDriveById(Guid? id)
        {
            var storageDrive = _storageDriveRepository.Find(null).Items?.Where(q => q.Id == id).FirstOrDefault();
            if (storageDrive == null)
                throw new EntityDoesNotExistException("Storage drive could not be found");
            return storageDrive;
        }

        private void DeleteFolder(StorageFolder folder)
        {
            //delete folder in database
            _storageFolderRepository.SoftDelete(folder.Id.Value);
            _webhookPublisher.PublishAsync("Files.FolderDeleted", folder.Id.ToString(), folder.Name);
        }

        private List<Guid?> GetParentIds(string[] pathArray)
        {
            List<Guid?> parentIds = new List<Guid?>();
            foreach (var folderName in pathArray)
            {
                var folder = _storageFolderRepository.Find(null).Items?.Where(q => q.Name.ToLower() == folderName.ToLower()).FirstOrDefault();
                if (folder != null)
                {
                    Guid? folderId = folder?.Id;
                    Guid? driveId = folder.StorageDriveId;
                    if (folderName == "Files")
                        folderId = driveId;
                    if (folderId != null)
                        parentIds.Add(folderId);
                }
            }
            return parentIds;
        }

        private bool CheckFolderHasChild(Guid? id)
        {
            bool hasChild = true;
            var folderChildren = _storageFolderRepository.Find(null).Items?.Where(q => q.ParentFolderId == id);
            var fileChildren = _storageFileRepository.Find(null).Items.Where(q => q.StorageFolderId == id);
            if (!folderChildren.Any() && !fileChildren.Any())
                hasChild = false;

            return hasChild;
        }

        private void RenameChildFilesFolders(Guid? entityId, string oldPath, string newPath, int index)
        {
            var childFiles = _storageFileRepository.Find(null, q => q.StorageFolderId == entityId).Items;
            if (childFiles.Any())
            {
                foreach (var file in childFiles)
                {
                    string oldFilePath = file.StoragePath;
                    string[] pathArray = oldPath.Split(Path.DirectorySeparatorChar);
                    string newFilePath = GetNewPath(file.StoragePath, oldPath, newPath, index);
                    file.StoragePath = newFilePath;
                    _storageFileRepository.Update(file);
                    _webhookPublisher.PublishAsync("Files.FileUpdated", file.Id.ToString(), file.Name);
                }
            }
            var childFolders = _storageFolderRepository.Find(null).Items?.Where(q => q.ParentFolderId == entityId);
            if (childFolders.Any())
            {
                foreach (var folder in childFolders)
                {
                    bool hasChild = CheckFolderHasChild(folder.Id);
                    string oldFolderPath = folder.StoragePath;
                    string[] pathArray = oldPath.Split(Path.DirectorySeparatorChar);
                    string newFolderPath = GetNewPath(folder.StoragePath, oldPath, newPath, index);
                    folder.StoragePath = newFolderPath;
                    _storageFolderRepository.Update(folder);
                    _webhookPublisher.PublishAsync("Files.FolderUpdated", folder.Id.ToString(), folder.Name);
                    if (hasChild)
                        RenameChildFilesFolders(folder.Id, oldPath, newPath, index);
                }
            }
        }

        private string GetNewPath(string path, string oldPath, string newPath, int index)
        {
            //string[] pathArray = path.Split(Path.DirectorySeparatorChar);
            //string segment = pathArray[index];
            //segment = segment.Replace(oldPath, newPath);
            //pathArray.SetValue(segment, index);
            //path = string.Join(Path.DirectorySeparatorChar, pathArray);
            path = path.Replace(oldPath, newPath);

            return path;
        }

        private void ExistingFolderCheck(string path)
        {
            var folder = _storageFolderRepository.Find(null).Items?.Where(q => q.StoragePath == path && q.IsDeleted == false).FirstOrDefault();
            if (folder != null)
                throw new EntityAlreadyExistsException($"Folder with path {path} already exists in current folder");
        }

        private void ExistingFileCheck(string path)
        {
            var file = _storageFileRepository.Find(null).Items?.Where(q => q.StoragePath == path && q.IsDeleted == false).FirstOrDefault();
            if (file != null)
                throw new EntityAlreadyExistsException($"File with path {path} already exists in current folder");
        }

        private void UpdateFolderSize(Guid? oldParentFolderId, string storagePath, StorageFile storageFile = null, StorageFolder storageFolder = null)
        {
            var oldParentFolder = _storageFolderRepository.GetOne((Guid)oldParentFolderId);
            string oldStoragePath;
            if (oldParentFolder == null)
            {
                var oldParentDrive = _storageDriveRepository.GetOne(oldParentFolderId.Value);
                oldStoragePath = oldParentDrive.StoragePath;
            }
            else oldStoragePath = oldParentFolder.StoragePath;

            long? size;
            if (storageFile != null)
                size = storageFile.SizeInBytes;
            else
                size = storageFolder.SizeInBytes;

            var oldParentPathArray = oldStoragePath.Split(Path.DirectorySeparatorChar);
            var newParentPathArray = storagePath.Split(Path.DirectorySeparatorChar);
            var oldParentPathList = new List<string>();
            var newParentPathList = new List<string>();

            foreach (var folder in oldParentPathArray)
            {
                if (!newParentPathArray.Contains(folder))
                    oldParentPathList.Add(folder);
            }
            foreach (var folder in newParentPathArray)
            {
                if (!oldParentPathArray.Contains(folder))
                    newParentPathList.Add(folder);
            }

            string oldParentPath = string.Join(Path.DirectorySeparatorChar, oldParentPathList);
            string newParentPath = string.Join(Path.DirectorySeparatorChar, newParentPathList);

            //remove size in bytes in old parent folders (only those that do not overlap in new parent folders list)
            AddBytesToParentFolders(oldParentPath, -size);

            //add size in bytes of new parent folders (only those that do not overlap in old parent folders list)
            AddBytesToParentFolders(newParentPath, size);
        } 

        private void CopyChildFilesFolders(StorageFolder storageFolder, string oldPath, string newPath, StorageDrive drive, Guid? oldParentFolderId,
            Guid? entityId, string parentFolderPath)
        {
            //copy files
            var files = _storageFileRepository.Find(null, q => q.StorageFolderId == storageFolder.Id).Items;
            foreach (StorageFile file in files)
            {
                //copy all files to new directory
                string oldFilePath = file.StoragePath;
                string newFilePath = oldFilePath.Replace(oldPath, newPath);

                //add new file entity in repository
                string shortPath = GetShortPath(newFilePath);

                using (var stream = IOFile.OpenRead(file.StorageLocation))
                {
                    IFormFile formFile = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                    var parentFolderEntity = _storageFolderRepository.Find(null).Items?.Where(q => q.StoragePath == shortPath).FirstOrDefault();

                    var newFile = new FileFolderViewModel();
                    newFile = newFile.Map(file, shortPath);
                    newFile.StoragePath = shortPath;
                    newFile.FullStoragePath = newFilePath;
                    newFile.ParentId = parentFolderEntity.Id;

                    //create new file and file attributes in database
                    newFile = SaveFile(newFile, formFile, drive);
                }
            }

            //create subfolders
            var folders = _storageFolderRepository.Find(null, q => q.ParentFolderId == oldParentFolderId).Items;
            foreach (StorageFolder folder in folders)
            {
                string shortPath = folder.StoragePath.Replace(folder.StoragePath, storageFolder.StoragePath);
                CheckFolderExists(shortPath);
                string newFolderPath = Path.Combine(shortPath, folder.Name);

                //add new folder entity in repository
                var parentFolderEntity = _storageFolderRepository.Find(null, q => q.StoragePath == shortPath).Items?.FirstOrDefault();

                var folderCopy = new StorageFolder();
                folder.ParentFolderId = parentFolderEntity.Id;
                folderCopy.StoragePath = newFolderPath;
                folderCopy.CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name;
                folderCopy.CreatedOn = DateTime.UtcNow;
                folderCopy.Name = folder.Name;
                folderCopy.OrganizationId = folder.OrganizationId;
                folderCopy.SizeInBytes = folder.SizeInBytes;
                folderCopy.StorageDriveId = folder.StorageDriveId;
                _storageFolderRepository.Add(folder);
                _webhookPublisher.PublishAsync("Files.NewFolderCreated", folder.Id.ToString(), folder.Name);

                bool hasChild = CheckFolderHasChild(entityId);
                if (hasChild)
                    CopyChildFilesFolders(folder, oldPath, newPath, drive, folder.Id, entityId, folderCopy.StoragePath);
            }

            //add size in bytes of each folder to new parent folder and storage drive
            long? size = storageFolder.SizeInBytes;
            AddBytesToParentFolders(parentFolderPath, size);

            drive.StorageSizeInBytes += size;
            _storageDriveRepository.Update(drive);
        }
        #endregion
    }
}