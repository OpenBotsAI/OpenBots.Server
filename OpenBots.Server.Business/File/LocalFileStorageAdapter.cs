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
    public class LocalFileStorageAdapter : IFileStorageAdapter
    {
        private readonly IServerFileRepository _serverFileRepository;
        private readonly IFileAttributeRepository _fileAttributeRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrganizationManager _organizationManager;
        private readonly IServerFolderRepository _serverFolderRepository;
        private readonly IServerDriveRepository _serverDriveRepository;
        private readonly IWebhookPublisher _webhookPublisher;
        private readonly IDirectoryManager _directoryManager;
        private readonly IAuditLogRepository _auditLogRepository;

        public IConfiguration Configuration { get; }

        public LocalFileStorageAdapter(
            IServerFileRepository serverFileRepository,
            IFileAttributeRepository fileAttributeRepository,
            IHttpContextAccessor httpContextAccessor,
            IOrganizationManager organizationManager,
            IServerFolderRepository serverFolderRepository,
            IServerDriveRepository serverDriveRepository,
            IConfiguration configuration,
            IWebhookPublisher webhookPublisher,
            IDirectoryManager directoryManager,
            IAuditLogRepository auditLogRepository)
        {
            _fileAttributeRepository = fileAttributeRepository;
            _serverFileRepository = serverFileRepository;
            _httpContextAccessor = httpContextAccessor;
            _organizationManager = organizationManager;
            _serverFolderRepository = serverFolderRepository;
            _serverDriveRepository = serverDriveRepository;
            _webhookPublisher = webhookPublisher;
            _directoryManager = directoryManager;
            _auditLogRepository = auditLogRepository;
            Configuration = configuration;
        }

        public PaginatedList<FileFolderViewModel> GetFilesFolders(bool? isFile = null, string driveName = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            var filesFolders = new PaginatedList<FileFolderViewModel>();
            var files = new List<FileFolderViewModel>();
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            Guid? driveId = GetDriveId(driveName);

            if (isFile.Equals(true))
            {
                //get all files
                filesFolders = _serverFileRepository.FindAllView(driveId, predicate, sortColumn, direction, skip, take);
            }
            else if (isFile.Equals(false))
            {
                //get all folders
                filesFolders = _serverFolderRepository.FindAllView(driveId, predicate, sortColumn, direction, skip, take);
            }
            else
            {
                //get all folders and files
                filesFolders = _serverFolderRepository.FindAllFilesFoldersView(driveId, predicate, sortColumn, direction, skip, take);
            }

            return filesFolders;
        }

        public List<FileFolderViewModel> AddFileFolder(FileFolderViewModel request, string driveName)
        {
            var fileFolderList = new List<FileFolderViewModel>();
            var newFileFolder = new FileFolderViewModel();

            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            ServerDrive drive = GetDriveByName(driveName);

            if ((bool)request.IsFile)
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
                Guid agentId;
                if (Guid.TryParse(lastFolder, out agentId))
                    path = GetShortPath(path);
                AddBytesToParentFolders(path, filesSizeInBytes);

                //add size in bytes to server drive
                AddBytesToServerDrive(drive, filesSizeInBytes);
            }
            else
            {
                //add folder
                string shortPath = request.StoragePath;
                string path = Path.Combine(shortPath, request.Name);
                request.FullStoragePath = path;
                var parentId = GetFolderId(shortPath, driveName);
                var id = Guid.NewGuid();
                ExistingFolderCheck(path);
                Guid? organizationId = _organizationManager.GetDefaultOrganization().Id;
                long? size = 0;
                if (request.Size != null)
                    size = request.Size;

                ServerFolder serverFolder = new ServerFolder()
                {
                    Id = id,
                    ParentFolderId = parentId,
                    CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                    CreatedOn = DateTime.UtcNow,
                    Name = request.Name,
                    SizeInBytes = size,
                    StorageDriveId = drive.Id,
                    StoragePath = path,
                    OrganizationId = organizationId
                };

                bool directoryExists = CheckDirectoryExists(shortPath);
                if (directoryExists)
                {
                    //create directory and add server folder
                    _directoryManager.CreateDirectory(path);
                    _serverFolderRepository.Add(serverFolder);
                    _webhookPublisher.PublishAsync("Files.NewFolderCreated", serverFolder.Id.ToString(), serverFolder.Name);

                    var hasChild = false;
                    newFileFolder = newFileFolder.Map(serverFolder, request.StoragePath, hasChild);
                    fileFolderList.Add(newFileFolder);
                }
                else
                    throw new DirectoryNotFoundException("Storage path could not be found");
            }
            return fileFolderList;
        }

        public void AddBytesToServerDrive(ServerDrive serverDrive, long? size)
        {
            //add to storage size in bytes property in server drive
            serverDrive.StorageSizeInBytes += size;
            _serverDriveRepository.Update(serverDrive);
            _webhookPublisher.PublishAsync("Files.DriveUpdated", serverDrive.Id.ToString(), serverDrive.Name);
        }

        public void AddBytesToParentFolders(string path, long? size)
        {
            var pathArray = path.Split(Path.DirectorySeparatorChar);
            List<Guid?> parentIds = GetParentIds(pathArray);
            foreach (var serverFolderId in parentIds)
            {
                var folder = _serverFolderRepository.Find(null).Items?.Where(q => q.Id == serverFolderId).FirstOrDefault();
                if (folder != null)
                {
                    folder.SizeInBytes += size;
                    _serverFolderRepository.Update(folder);
                    _webhookPublisher.PublishAsync("Files.FolderUpdated", folder.Id.ToString(), folder.Name);
                }
            }
        }

        public FileFolderViewModel SaveFile(FileFolderViewModel request, IFormFile file, ServerDrive drive)
        {
            Guid? id = Guid.NewGuid();
            string shortPath = request.StoragePath;
            string path = request.FullStoragePath;
            Guid? organizationId = _organizationManager.GetDefaultOrganization().Id;
            ExistingFileCheck(path);

            //upload file to local server
            bool directoryExists = CheckDirectoryExists(shortPath);
            if (!directoryExists)
                throw new DirectoryNotFoundException("Storage path could not be found");

            if (file.Length <= 0 || file.Equals(null)) throw new Exception("No file exists");
            if (file.Length > 0)
            {
                using (var stream = new FileStream(path, FileMode.Create))
                    file.CopyTo(stream);

                ConvertToBinaryObject(path);
            }

            Guid? folderId = GetFolderId(shortPath, drive.Name);
            var hash = GetHash(path);
            Guid? driveId = drive.Id;

            //add file properties to server file entity
            var serverFile = new ServerFile()
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
                ServerDriveId = drive.Id
            };
            _serverFileRepository.Add(serverFile);
            _webhookPublisher.PublishAsync("Files.NewFileCreated", serverFile.Id.ToString(), serverFile.Name);

            //add file attribute entities
            var attributes = new Dictionary<string, int>()
            {
                { FileAttributes.StorageCount.ToString(), 1 },
                { FileAttributes.RetrievalCount.ToString(), 0 },
                { FileAttributes.AppendCount.ToString(), 0 }
            };

            List<FileAttribute> fileAttributes = new List<FileAttribute>();
            foreach (var attribute in attributes)
            {
                var fileAttribute = new FileAttribute()
                {
                    ServerFileId = id,
                    AttributeValue = attribute.Value,
                    CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                    CreatedOn = DateTime.UtcNow,
                    DataType = attribute.Value.GetType().ToString(),
                    Name = attribute.Key,
                    OrganizationId = organizationId,
                    ServerDriveId = driveId
                };
                _fileAttributeRepository.Add(fileAttribute);
                fileAttributes.Add(fileAttribute);
            }

            var viewModel = new FileFolderViewModel();
            viewModel = viewModel.Map(serverFile, shortPath);
            return viewModel;
        }

        public async void UpdateFile(FileFolderViewModel request)
        {
            Guid entityId = (Guid)request.Id;
            var serverFile = _serverFileRepository.GetOne(entityId);
            if (serverFile == null) throw new EntityDoesNotExistException("Server file could not be found");

            var file = request.Files[0];
            string path = request.StoragePath;
            string oldPath = request.FullStoragePath;
            Guid? organizationId = _organizationManager.GetDefaultOrganization().Id;

            long? size = serverFile.SizeInBytes;

            //update file attribute entities
            List<FileAttribute> fileAttributes = new List<FileAttribute>();
            var attributes = _fileAttributeRepository.Find(null).Items?.Where(q => q.ServerFileId == entityId);
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    if (attribute.Name == FileAttributes.AppendCount.ToString() || attribute.Name == FileAttributes.StorageCount.ToString())
                    {
                        attribute.AttributeValue += 1;
                        _fileAttributeRepository.Update(attribute);
                    }
                    fileAttributes.Add(attribute);
                }
            }
            else throw new EntityDoesNotExistException("File attribute entities could not be found for this file");

            //update file stored in server
            string shortPath = GetShortPath(path);
            bool directoryExists = CheckDirectoryExists(shortPath);
            if (!directoryExists)
                throw new DirectoryNotFoundException("Storage path could not be found");

            if (file.Length > 0)
            {
                if (oldPath != null)
                    IOFile.Delete(oldPath);

                var stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                using (stream)
                {
                    file.CopyTo(stream);
                }

                ConvertToBinaryObject(path);
            }

            var hash = GetHash(path);

            //update server file entity properties
            serverFile.ContentType = file.ContentType;
            serverFile.HashCode = hash;
            serverFile.Name = file.FileName;
            serverFile.OrganizationId = organizationId;
            serverFile.SizeInBytes = file.Length;
            serverFile.StoragePath = request.StoragePath;
            serverFile.FileAttributes = fileAttributes;

            _serverFileRepository.Update(serverFile);
            _webhookPublisher.PublishAsync("Files.FileUpdated", serverFile.Id.ToString(), serverFile.Name);

            //update size in bytes of server folders
            var drive = GetDriveById(serverFile.ServerDriveId);
            if (serverFile.StorageFolderId != drive.Id)
                AddBytesToParentFolders(request.StoragePath, serverFile.SizeInBytes);

            //update size in bytes in server drive
            size = request.Files[0].Length - size;
            AddBytesToServerDrive(drive, size);
        }

        public long? DeleteFile(ServerFile serverFile)
        {
            //remove file attribute entities
            var attributes = _fileAttributeRepository.Find(null).Items?.Where(q => q.ServerFileId == serverFile.Id);
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                    _fileAttributeRepository.SoftDelete((Guid)attribute.Id);
            }

            //remove file
            IOFile.Delete(serverFile.StoragePath);

            //remove server file entity
            _serverFileRepository.SoftDelete((Guid)serverFile.Id);
            _webhookPublisher.PublishAsync("Files.FileDeleted", serverFile.Id.ToString(), serverFile.Name);

            long? size = -serverFile.SizeInBytes;
            return size;
        }

        protected enum FileAttributes
        {
            StorageCount,
            RetrievalCount,
            AppendCount
        }

        protected bool CheckDirectoryExists(string path)
        {
            if (_directoryManager.Exists(path))
                return true;
            else
                return false;
        }

        protected string GetHash(string path)
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

        protected void ConvertToBinaryObject(string filePath)
        {
            byte[] bytes = IOFile.ReadAllBytes(filePath);
            IOFile.WriteAllBytes(filePath, bytes);
        }

        public FileFolderViewModel GetFileFolderViewModel(string id, string driveName)
        {
            ServerFolder folder = new ServerFolder();
            var fileFolder = new FileFolderViewModel();
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            Guid? driveId = GetDriveId(driveName);
            var file = _serverFileRepository.Find(null).Items?.Where(q => q.Id.ToString() == id && q.ServerDriveId == driveId).FirstOrDefault();

            if (file != null)
            {
                var serverFolder = _serverFolderRepository.Find(null).Items?.Where(q => q.Id == file.StorageFolderId).FirstOrDefault();
                Guid? folderId = Guid.Empty;
                string storagePath = string.Empty;
                if (serverFolder != null)
                {
                    folderId = serverFolder.Id;
                    storagePath = serverFolder.StoragePath;
                }
                else
                    storagePath = GetDriveById(file.ServerDriveId).Name;

                fileFolder.Id = file.Id;
                fileFolder.Name = file.Name;
                fileFolder.ContentType = file.ContentType;
                fileFolder.StoragePath = storagePath;
                fileFolder.CreatedBy = file.CreatedBy;
                fileFolder.CreatedOn = file.CreatedOn;
                fileFolder.UpdatedOn = file.UpdatedOn;
                fileFolder.FullStoragePath = file.StoragePath;
                fileFolder.Size = file.SizeInBytes;
                fileFolder.HasChild = false;
                fileFolder.IsFile = true;
                fileFolder.ParentId = file.StorageFolderId;
                fileFolder.StorageDriveId = driveId;
                fileFolder.Hash = file.HashCode;
            }
            else
            {
                folder = _serverFolderRepository.Find(null).Items?.Where(q => q.Id.ToString() == id && q.StorageDriveId == driveId).FirstOrDefault();

                if (folder == null)
                    throw new EntityDoesNotExistException($"File or folder does not exist");

                var pathArray = folder.StoragePath.Split(Path.DirectorySeparatorChar);
                var shortPathArray = new string[pathArray.Length - 1];
                for (int i = 0; i < pathArray.Length - 1; i++)
                {
                    string folderName = pathArray[i];
                    shortPathArray.SetValue(folderName, i);
                }

                bool hasChild = CheckFolderHasChild(folder.Id);

                fileFolder.Id = folder.Id;
                fileFolder.Name = folder.Name;
                fileFolder.ContentType = "Folder";
                fileFolder.StoragePath = string.Join(Path.DirectorySeparatorChar, shortPathArray);
                fileFolder.CreatedBy = folder.CreatedBy;
                fileFolder.CreatedOn = folder.CreatedOn;
                fileFolder.FullStoragePath = folder.StoragePath;
                fileFolder.Size = folder.SizeInBytes;
                fileFolder.HasChild = hasChild;
                fileFolder.IsFile = false;
                fileFolder.ParentId = folder.ParentFolderId;
                fileFolder.StorageDriveId = driveId;
            }

            return fileFolder;
        }

        public Guid? GetDriveId(string driveName)
        {
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            ServerDrive drive = GetDriveByName(driveName);
            Guid? driveId;
            if (drive != null)
                driveId = drive.Id;
            else throw new EntityDoesNotExistException($"Drive {driveName} could not be found or does not exist");

            return driveId;
        }

        public int? GetFileCount(string driveName)
        {
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            Guid? driveId = GetDriveId(driveName);
            var files = _serverFileRepository.Find(null).Items?.Where(q => q.ServerDriveId == driveId);
            int? count = files.Count();
            return count;
        }

        public int? GetFolderCount(string driveName)
        {
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            Guid? driveId = GetDriveId(driveName);
            var folders = _serverFolderRepository.Find(null).Items?.Where(q => q.StorageDriveId == driveId);
            int? count = folders.Count();
            return count;
        }

        public ServerFolder GetFolder(string path, string driveName)
        {
            Guid? driveId = GetDriveId(driveName);
            var serverFolder = _serverFolderRepository.Find(null).Items?.Where(q => q.StoragePath.ToLower() == path.ToLower() && q.StorageDriveId == driveId).FirstOrDefault();
            if (serverFolder == null)
                return null;
            return serverFolder;
        }

        public Guid? GetFolderId(string path, string driveName)
        {
            var folder = GetFolder(path, driveName);
            Guid? folderId = folder?.Id;
            if (folderId == null)
            {
                if (string.IsNullOrEmpty(driveName))
                    driveName = "Files";
                var serverDrive = GetDriveByName(driveName);
                if (serverDrive != null)
                    folderId = serverDrive.Id;
                else throw new EntityDoesNotExistException("Drive could not be found");
            }

            return folderId;
        }

        public ServerDrive GetDriveById(Guid? id)
        {
            var serverDrive = _serverDriveRepository.Find(null).Items?.Where(q => q.Id == id).FirstOrDefault();
            if (serverDrive == null)
                throw new EntityDoesNotExistException("Server drive could not be found");
            return serverDrive;
        }

        public ServerDrive GetDriveByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = "Files";
            var serverDrive = _serverDriveRepository.Find(null).Items?.Where(q => q.Name == name).FirstOrDefault();
            if (serverDrive == null)
                throw new EntityDoesNotExistException("Server drive could not be found");
            return serverDrive;
        }

        public void DeleteFolder(ServerFolder folder)
        {
            //delete folder in directory
            _directoryManager.Delete(folder.StoragePath);

            //delete folder in database
            _serverFolderRepository.SoftDelete((Guid)folder.Id);
            _webhookPublisher.PublishAsync("Files.FolderDeleted", folder.Id.ToString(), folder.Name);
        }

        public List<Guid?> GetParentIds(string[] pathArray)
        {
            List<Guid?> parentIds = new List<Guid?>();
            foreach (var folderName in pathArray)
            {
                var folder = _serverFolderRepository.Find(null).Items?.Where(q => q.Name.ToLower() == folderName.ToLower()).FirstOrDefault();
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

        public async Task<FileFolderViewModel> ExportFile(string id, string driveName)
        {
            Guid entityId = Guid.Parse(id);
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            Guid? driveId = GetDriveId(driveName);
            var file = _serverFileRepository.GetOne(entityId);
            var folder = _serverFolderRepository.GetOne(entityId);
            bool isFile = true;

            if (file == null && folder == null)
                throw new EntityDoesNotExistException("No file or folder found to export");

            if (file == null && folder != null)
                isFile = false;

            var fileFolder = new FileFolderViewModel();

            if (isFile)
            {
                if (driveId != file.ServerDriveId) throw new EntityDoesNotExistException($"File {file.Name} does not exist in current drive {driveName}");

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
                var stream = new FileStream(file.StoragePath, FileMode.Open, FileAccess.Read);

                fileFolder.StoragePath = file.StoragePath;
                fileFolder.Name = file.Name;
                fileFolder.ContentType = file.ContentType;
                fileFolder.Size = file.SizeInBytes;
                fileFolder.Id = file.Id;
                fileFolder.Content = stream;

                await stream.FlushAsync();

                //update file attribute: retrieval count
                var retrievalFileAttribute = _fileAttributeRepository.Find(null).Items?.Where(q => q.ServerFileId == file.Id && q.Name == FileAttributes.RetrievalCount.ToString()).FirstOrDefault();
                if (retrievalFileAttribute != null)
                {
                    retrievalFileAttribute.AttributeValue += 1;
                    _fileAttributeRepository.Update(retrievalFileAttribute);
                }
            }
            else
                throw new EntityOperationException("Folders cannot be exported at this time");

            return fileFolder;
        }

        public bool CheckFolderHasChild(Guid? id)
        {
            bool hasChild = true;
            var folderChildren = _serverFolderRepository.Find(null).Items?.Where(q => q.ParentFolderId == id);
            var fileChildren = _serverFileRepository.Find(null).Items.Where(q => q.StorageFolderId == id);
            if (!folderChildren.Any() && !fileChildren.Any())
                hasChild = false;

            return hasChild;
        }

        public FileFolderViewModel DeleteFileFolder(string id, string driveName = null)
        {
            FileFolderViewModel fileFolder = new FileFolderViewModel();
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            Guid? driveId = GetDriveId(driveName);
            var serverFile = _serverFileRepository.Find(null).Items?.Where(q => q.Id.ToString() == id && q.ServerDriveId == driveId).FirstOrDefault();
            var serverFolder = new ServerFolder();
            if (serverFile != null)
            {
                string shortPath = GetShortPath(serverFile.StoragePath);
                fileFolder = fileFolder.Map(serverFile, shortPath);
                DeleteFile(serverFile);
            }
            else if (serverFile == null)
            {
                serverFolder = _serverFolderRepository.Find(null).Items?.Where(q => q.Id.ToString() == id && q.StorageDriveId == driveId).FirstOrDefault();
                if (serverFolder != null)
                {
                    string shortPath = GetShortPath(serverFolder.StoragePath);
                    bool hasChild = CheckFolderHasChild(serverFolder.Id);
                    fileFolder = fileFolder.Map(serverFolder, shortPath, hasChild);
                    if (serverFolder.StoragePath.Contains("Queue Item Attachments") && !hasChild)
                        DeleteFolder(serverFolder);
                    else if (serverFolder.SizeInBytes != 0)
                        throw new EntityOperationException("Folder cannot be deleted because it has files inside");
                    else if (hasChild)
                        throw new EntityOperationException("Folder cannot be deleted because it has folders inside");
                    else DeleteFolder(serverFolder);
                }
                else
                    throw new EntityDoesNotExistException($"Folder with id '{id}' could not be found");
            }
            else
                throw new EntityDoesNotExistException($"File with id '{id}' could not be found");

            return fileFolder;
        }

        public FileFolderViewModel RenameFileFolder(string id, string name, string driveName = null)
        {
            Guid? entityId = Guid.Parse(id);
            var fileFolder = new FileFolderViewModel();
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            Guid? driveId = GetDriveId(driveName);
            var serverFile = _serverFileRepository.Find(null).Items?.Where(q => q.Id == entityId && q.ServerDriveId == driveId).FirstOrDefault();
            var serverFolder = new ServerFolder();

            if (serverFile != null)
            {
                //rename file
                string oldPath = serverFile.StoragePath;
                var newNameArray = serverFile.Name.Split(".");
                var contentType = newNameArray[newNameArray.Length - 1];
                string newName = name + "." + contentType;
                int index = oldPath.Split(Path.DirectorySeparatorChar).Length - 1;
                string newPath = GetNewPath(serverFile.StoragePath, serverFile.Name, newName, index);
                ExistingFileCheck(newPath);
                var newPathArray = newPath.Split(Path.DirectorySeparatorChar);
                var shortPathArray = new string[newPathArray.Length - 1];
                for (int i = 0; i < newPathArray.Length - 1; i++)
                {
                    string folderName = newPathArray[i];
                    shortPathArray.SetValue(folderName, i);
                }
                string shortPath = string.Join(Path.DirectorySeparatorChar, shortPathArray);

                serverFile.Name = newName;
                serverFile.StoragePath = newPath;
                _serverFileRepository.Update(serverFile);

                IOFile.Move(oldPath, newPath);
                _webhookPublisher.PublishAsync("Files.FileUpdated", id, serverFile.Name);

                fileFolder = fileFolder.Map(serverFile, shortPath);

                //update append file attribute
                var appendAttribute = _fileAttributeRepository.Find(null).Items?.Where(q => q.Name == FileAttributes.AppendCount.ToString() && q.ServerFileId == serverFile.Id).FirstOrDefault();
                if (appendAttribute != null)
                    appendAttribute.AttributeValue += 1;
                _fileAttributeRepository.Update(appendAttribute);
            }
            else if (serverFile == null)
            {
                serverFolder = _serverFolderRepository.Find(null).Items?.Where(q => q.Id == entityId && q.StorageDriveId == driveId).FirstOrDefault(); 
                if (serverFolder != null)
                {
                    //rename folder
                    bool hasChild = CheckFolderHasChild(entityId);
                    string oldPath = serverFolder.StoragePath;
                    string oldName = serverFolder.Name;
                    int index = oldPath.Split(Path.DirectorySeparatorChar).Length - 1;
                    string newPath = GetNewPath(serverFolder.StoragePath, oldName, name, index);
                    ExistingFolderCheck(newPath);
                    var newPathArray = newPath.Split(Path.DirectorySeparatorChar);
                    string shortPath = GetShortPath(newPath);

                    //move folder to new directory path
                    _directoryManager.Move(oldPath, newPath);
                    serverFolder.Name = name;
                    serverFolder.StoragePath = newPath;
                    _serverFolderRepository.Update(serverFolder);
                    _webhookPublisher.PublishAsync("Files.FolderUpdated", id, serverFolder.Name);

                    //rename all files and folders underneath it
                    if (hasChild)
                        RenameChildFilesFolders(entityId, oldName, name, index);
                    fileFolder = fileFolder.Map(serverFolder, shortPath, hasChild);
                }
                else
                    throw new EntityDoesNotExistException($"Folder or file with id '{id}' could not be found");
            }

            return fileFolder;
        }

        public void RenameChildFilesFolders(Guid? entityId, string oldPath, string newPath, int index)
        {
            var childFiles = _serverFileRepository.Find(null).Items?.Where(q => q.StorageFolderId == entityId);
            if (childFiles.Any())
            {
                foreach (var file in childFiles)
                {
                    string oldFilePath = file.StoragePath;
                    string[] pathArray = oldPath.Split(Path.DirectorySeparatorChar);
                    string newFilePath = GetNewPath(file.StoragePath, oldPath, newPath, index);
                    file.StoragePath = newFilePath;
                    _serverFileRepository.Update(file);
                    if (IOFile.Exists(oldPath))
                        IOFile.Move(oldFilePath, newFilePath);
                    _webhookPublisher.PublishAsync("Files.FileUpdated", file.Id.ToString(), file.Name);
                }
            }
            var childFolders = _serverFolderRepository.Find(null).Items?.Where(q => q.ParentFolderId == entityId);
            if (childFolders.Any())
            {
                foreach (var folder in childFolders)
                {
                    bool hasChild = CheckFolderHasChild(folder.Id);
                    string oldFolderPath = folder.StoragePath;
                    string[] pathArray = oldPath.Split(Path.DirectorySeparatorChar);
                    string newFolderPath = GetNewPath(folder.StoragePath, oldPath, newPath, index);
                    folder.StoragePath = newFolderPath;
                    _serverFolderRepository.Update(folder);
                    if (_directoryManager.Exists(oldPath))
                        _directoryManager.Move(oldFolderPath, newFolderPath);
                    _webhookPublisher.PublishAsync("Files.FolderUpdated", folder.Id.ToString(), folder.Name);
                    if (hasChild)
                        RenameChildFilesFolders(folder.Id, oldPath, newPath, index);
                }
            }
        }

        public string GetNewPath(string path, string oldPath, string newPath, int index)
        {
            string[] pathArray = path.Split(Path.DirectorySeparatorChar);
            string segment = pathArray[index];
            segment = segment.Replace(oldPath, newPath);
            pathArray.SetValue(segment, index);
            path = string.Join(Path.DirectorySeparatorChar, pathArray);
            
            return path;
        }

        public FileFolderViewModel MoveFileFolder(string fileFolderId, string parentFolderId, string driveName = null)
        {
            Guid? entityId = Guid.Parse(fileFolderId);
            Guid? parentId = Guid.Parse(parentFolderId);
            var fileFolder = new FileFolderViewModel();
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            Guid? driveId = GetDriveId(driveName);
            var newParentFolder = _serverFolderRepository.GetOne((Guid)parentId);
            string parentFolderPath = newParentFolder.StoragePath;

            var serverFile = _serverFileRepository.Find(null).Items?.Where(q => q.Id == entityId && q.ServerDriveId == driveId).FirstOrDefault();
            var serverFolder = new ServerFolder();

            if (serverFile != null)
            {
                //move file
                Guid? oldParentFolderId = serverFile.StorageFolderId;
                string oldPath = serverFile.StoragePath;
                string newPath = Path.Combine(parentFolderPath, serverFile.Name);
                ExistingFileCheck(newPath);

                serverFile.StoragePath = newPath;
                serverFile.StorageFolderId = parentId;
                _serverFileRepository.Update(serverFile);

                IOFile.Move(oldPath, newPath);
                _webhookPublisher.PublishAsync("Files.FileUpdated", fileFolderId, serverFile.Name);

                fileFolder = fileFolder.Map(serverFile, parentFolderPath);

                //update append file attribute
                var appendAttribute = _fileAttributeRepository.Find(null).Items?.Where(q => q.Name == FileAttributes.AppendCount.ToString() && q.ServerFileId == serverFile.Id).FirstOrDefault();
                if (appendAttribute != null)
                    appendAttribute.AttributeValue += 1;
                _fileAttributeRepository.Update(appendAttribute);

                //update new and old parent folder sizes
                UpdateFolderSize(oldParentFolderId, newParentFolder, serverFile);
            }
            else if (serverFile == null)
            {
                serverFolder = _serverFolderRepository.Find(null).Items?.Where(q => q.Id == entityId && q.StorageDriveId == driveId).FirstOrDefault();           //else 
                if (serverFolder != null)
                {
                    //move folder
                    bool hasChild = CheckFolderHasChild(entityId);
                    Guid? oldParentFolderId = serverFolder.ParentFolderId;
                    string oldPath = serverFolder.StoragePath;
                    string newPath = Path.Combine(parentFolderPath, serverFolder.Name);
                    int index = oldPath.Split(Path.DirectorySeparatorChar).Length - 1;
                    ExistingFolderCheck(newPath);

                    serverFolder.StoragePath = newPath;
                    serverFolder.ParentFolderId = parentId;
                    _serverFolderRepository.Update(serverFolder);
                    _webhookPublisher.PublishAsync("Files.FolderUpdated", serverFolder.Id.ToString(), serverFolder.Name);

                    //move folder to new directory path
                    _directoryManager.Move(oldPath, newPath);
                    _webhookPublisher.PublishAsync("Files.FolderUpdated", fileFolderId, serverFolder.Name);

                    //rename all files and folders underneath it
                    if (hasChild)
                        RenameChildFilesFolders(entityId, oldPath, newPath, index);
                    fileFolder = fileFolder.Map(serverFolder, parentFolderPath, hasChild);

                    //update new and old parent folder sizes
                    UpdateFolderSize(oldParentFolderId, newParentFolder, null, serverFolder);
                }
                else
                    throw new EntityDoesNotExistException($"Folder or file with id '{fileFolderId}' could not be found");
            }

            return fileFolder;
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

        public FileFolderViewModel CopyFileFolder(string fileFolderId, string parentFolderId, string driveName = null)
        {
            Guid? entityId = Guid.Parse(fileFolderId);
            Guid? parentId = Guid.Parse(parentFolderId);
            var fileFolder = new FileFolderViewModel();
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            var drive = GetDriveByName(driveName);
            Guid? driveId = drive.Id;
            var parentFolder = _serverFolderRepository.GetOne((Guid)parentId);
            string parentFolderPath = parentFolder.StoragePath;

            var serverFile = _serverFileRepository.Find(null).Items?.Where(q => q.Id == entityId && q.ServerDriveId == driveId).FirstOrDefault();
            var serverFolder = new ServerFolder();

            if (serverFile != null)
            {
                //copy file
                string oldPath = serverFile.StoragePath;
                string newPath = Path.Combine(parentFolderPath, serverFile.Name);

                using (var stream = IOFile.OpenRead(oldPath))
                {
                    IFormFile file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                    fileFolder = fileFolder.Map(serverFile, parentFolderPath);
                    fileFolder.StoragePath = parentFolderPath;
                    fileFolder.FullStoragePath = newPath;
                    fileFolder.UpdatedOn = null;
                    fileFolder.ParentId = parentId;
                    fileFolder.CreatedBy = null;
                    fileFolder.CreatedOn = null;
                    //create new file and file attributes in database
                    //create new file in server drive
                    fileFolder = SaveFile(fileFolder, file, drive);
                    var newFile = _serverFileRepository.GetOne((Guid)fileFolder.Id);

                    //add size in bytes of file to new parent folders and server drive
                    UpdateFolderSize(serverFile.StorageFolderId, parentFolder, newFile);

                    drive.StorageSizeInBytes += file.Length;
                    _serverDriveRepository.Update(drive);
                }
            }
            else if (serverFile == null)
            {
                serverFolder = _serverFolderRepository.Find(null).Items?.Where(q => q.Id == entityId && q.StorageDriveId == driveId).FirstOrDefault();           //else 
                if (serverFolder != null)
                {
                    string oldPath = serverFolder.StoragePath;
                    string newPath = Path.Combine(parentFolderPath, serverFolder.Name);
                    ExistingFolderCheck(newPath);

                    //create folder entity in database
                    serverFolder.ParentFolderId = parentId;
                    serverFolder.Id = Guid.NewGuid();
                    serverFolder.StoragePath = newPath;
                    serverFolder.CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name;
                    serverFolder.CreatedOn = DateTime.UtcNow;
                    serverFolder.Id = Guid.NewGuid();
                    serverFolder.UpdatedBy = null;
                    serverFolder.UpdatedOn = null;
                    _serverFolderRepository.Add(serverFolder);

                    //map server folder to file folder view model
                    bool hasChild = CheckFolderHasChild(entityId);
                    fileFolder.Map(serverFolder, parentFolderPath, hasChild);

                    //create subdirectory structure in destination    
                    foreach (string dir in Directory.GetDirectories(oldPath, "*", SearchOption.AllDirectories))
                    {
                        //create copied folders with the same name
                        var folder = _serverFolderRepository.Find(null).Items?.Where( q => q.StoragePath == dir).FirstOrDefault();
                        string shortPath = folder.StoragePath.Replace(folder.StoragePath, serverFolder.StoragePath);
                        CheckDirectoryExists(shortPath);
                        string newFolderPath = Path.Combine(shortPath, folder.Name);
                        _directoryManager.CreateDirectory(newFolderPath);

                        //add new folder entity in repository
                        var parentFolderEntity = _serverFolderRepository.Find(null).Items?.Where(q => q.StoragePath == shortPath).FirstOrDefault();
                        folder.ParentFolderId = parentFolderEntity.Id;
                        folder.Id = Guid.NewGuid();
                        folder.StoragePath = Path.Combine(shortPath, folder.Name);
                        folder.CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name;
                        folder.CreatedOn = DateTime.UtcNow;
                        folder.Id = Guid.NewGuid();
                        folder.UpdatedBy = null;
                        folder.UpdatedOn = null;
                        _serverFolderRepository.Add(folder);
                    }

                    foreach (string filePath in Directory.GetFiles(oldPath, "*", SearchOption.AllDirectories))
                    {
                        //copy all files to new directory
                        var file = _serverFileRepository.Find(null).Items?.Where(q => q.StoragePath == filePath).FirstOrDefault();
                        //string newFilePath = GetNewPath(file.StoragePath, file.StoragePath, serverFolder.StoragePath);
                        string oldFilePath = file.StoragePath;
                        string newFilePath = oldFilePath.Replace(oldPath, newPath);

                        //add new file entity in repository
                        string shortPath = GetShortPath(newFilePath);

                        using (var stream = IOFile.OpenRead(file.StoragePath))
                        {
                            IFormFile formFile = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                            var parentFolderEntity = _serverFolderRepository.Find(null).Items?.Where(q => q.StoragePath == shortPath).FirstOrDefault();
                            var newFile = new FileFolderViewModel();
                            newFile = newFile.Map(file, shortPath);
                            newFile.StoragePath = shortPath;
                            newFile.FullStoragePath = newFilePath;
                            newFile.UpdatedOn = null;
                            newFile.ParentId = parentFolderEntity.Id;
                            newFile.CreatedBy = null;
                            newFile.CreatedOn = null;
                            //create new file and file attributes in database
                            //create new file in server drive
                            newFile = SaveFile(newFile, formFile, drive);
                        }
                    }
                    //add size in bytes of each folder to new parent folder and server drive
                    long? size = serverFolder.SizeInBytes;
                    AddBytesToParentFolders(parentFolderPath, size);

                    drive.StorageSizeInBytes += size;
                    _serverDriveRepository.Update(drive);
                }
                else
                    throw new EntityDoesNotExistException($"Folder or file with id '{fileFolderId}' could not be found");
            }

            return fileFolder;
        }

        public void ExistingFolderCheck(string path)
        {
            var folder = _serverFolderRepository.Find(null).Items?.Where(q => q.StoragePath == path && q.IsDeleted == false).FirstOrDefault();
            if (folder != null)
                throw new EntityAlreadyExistsException($"Folder with path {path} already exists in current folder");
        }

        public void ExistingFileCheck(string path)
        {
            var file = _serverFileRepository.Find(null).Items?.Where(q => q.StoragePath == path && q.IsDeleted == false).FirstOrDefault();
            if (file != null)
                throw new EntityAlreadyExistsException($"File with path {path} already exists in current folder");
        }

        public void UpdateFolderSize(Guid? oldParentFolderId, ServerFolder newParentFolder, ServerFile serverFile = null, ServerFolder serverFolder = null)
        {
            var oldParentFolder = _serverFolderRepository.GetOne((Guid)oldParentFolderId);
            long? size;
            if (serverFile != null)
                size = serverFile.SizeInBytes;
            else
                size = serverFolder.SizeInBytes;

            var oldParentPathArray = oldParentFolder.StoragePath.Split(Path.DirectorySeparatorChar);
            var newParentPathArray = newParentFolder.StoragePath.Split(Path.DirectorySeparatorChar);
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

            //remove size in bytes in old parent foders (only those that do not overlap in new parent folders list)
            AddBytesToParentFolders(oldParentPath, -size);

            //add size in bytes of new parent folders (only those that do not overlap in old parent folders list)
            AddBytesToParentFolders(newParentPath, size);
        }

        public void AddBytesToFoldersAndDrive(List<FileFolderViewModel> files)
        {
            long? size = 0;
            var storagePath = files[0].FullStoragePath;
            var serverDriveId = files[0].StorageDriveId;
            foreach (var file in files)
                size -= file.Size;

            //update size in bytes in folder
            AddBytesToParentFolders(storagePath, size);

            //update size in bytes in server drive
            var drive = GetDriveById(serverDriveId);
            AddBytesToServerDrive(drive, size);
        }

        public FileFolderViewModel GetFileFolderByStoragePath(string storagePath, string driveName)
        {
            var fileView = new FileFolderViewModel();
            if (string.IsNullOrEmpty(driveName))
                driveName = "Files";
            var driveId = GetDriveId(driveName);
            var shortPath = GetShortPath(storagePath);
            var serverFile = _serverFileRepository.Find(null).Items?.Where(q => q.StoragePath == storagePath && q.ServerDriveId == driveId).FirstOrDefault();
            if (serverFile == null)
            {
                var serverFolder = _serverFolderRepository.Find(null).Items?.Where(q => q.StoragePath == storagePath && q.StorageDriveId == driveId).FirstOrDefault();
                if (serverFolder != null)
                {
                    bool hasChild = CheckFolderHasChild(serverFolder.Id);
                    fileView = fileView.Map(serverFolder, shortPath, hasChild);
                }
            }
            else
                fileView = fileView.Map(serverFile, storagePath);

            return fileView;
        }

        public ServerDrive AddServerDrive(string driveName)
        {
            Guid? organizationId = _organizationManager.GetDefaultOrganization().Id;

            var existingServerDrive = _serverDriveRepository.Find(null, q => q.Name == driveName).Items?.FirstOrDefault();
            if (existingServerDrive != null)
                throw new EntityAlreadyExistsException($"Drive with name {driveName} already exists");

            var serverDrive = new ServerDrive()
            {
                FileStorageAdapterType = "LocalFileStorage",
                CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                Name = driveName,
                OrganizationId = organizationId,
                StoragePath = driveName,
                StorageSizeInBytes = 0
            };
            _serverDriveRepository.Add(serverDrive);
            _directoryManager.CreateDirectory(driveName);

            _webhookPublisher.PublishAsync("Files.NewDriveCreated", serverDrive.Id.ToString(), serverDrive.Name);

            return serverDrive;
        }

        public Dictionary<Guid?, string> GetDriveNames(string adapterType)
        {
            var driveNames = new Dictionary<Guid?, string>();

            var drives = _serverDriveRepository.Find(null).Items.Where(q => q.FileStorageAdapterType == adapterType);

            if (drives == null)
                throw new EntityDoesNotExistException("No drives could be found");

            foreach (var drive in drives)
                driveNames.Add(drive.Id, drive.Name);

            return driveNames;
        }
    }
}