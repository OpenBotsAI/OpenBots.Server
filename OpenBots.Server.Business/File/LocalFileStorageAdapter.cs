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
        private readonly IServerFileRepository serverFileRepository;
        private readonly IFileAttributeRepository fileAttributeRepository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IOrganizationManager organizationManager;
        private readonly IServerFolderRepository serverFolderRepository;
        private readonly IServerDriveRepository serverDriveRepository;
        private readonly IWebhookPublisher webhookPublisher;
        private readonly IDirectoryManager directoryManager;
        private readonly IAuditLogRepository auditLogRepository;

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
            this.fileAttributeRepository = fileAttributeRepository;
            this.serverFileRepository = serverFileRepository;
            this.httpContextAccessor = httpContextAccessor;
            this.organizationManager = organizationManager;
            this.serverFolderRepository = serverFolderRepository;
            this.serverDriveRepository = serverDriveRepository;
            this.webhookPublisher = webhookPublisher;
            this.directoryManager = directoryManager;
            this.auditLogRepository = auditLogRepository;
            Configuration = configuration;
        }

        public PaginatedList<FileFolderViewModel> GetFilesFolders(bool? isFile = null, string driveName = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            var filesFolders = new PaginatedList<FileFolderViewModel>();
            var files = new List<FileFolderViewModel>();
            Guid? driveId = GetDriveId(driveName);

            if (isFile.Equals(true))
            {
                //get all files
                filesFolders = serverFileRepository.FindAllView(driveId, predicate, sortColumn, direction, skip, take);
            }
            else if (isFile.Equals(false))
            {
                //get all folders
                filesFolders = serverFolderRepository.FindAllView(driveId, predicate, sortColumn, direction, skip, take);
            }
            else
            {
                //get all folders and files
                filesFolders = serverFolderRepository.FindAllFilesFoldersView(driveId, predicate, sortColumn, direction, skip, take);
            }

            return filesFolders;
        }

        public List<FileFolderViewModel> AddFileFolder(FileFolderViewModel request, string driveName)
        {
            var fileFolderList = new List<FileFolderViewModel>();
            var newFileFolder = new FileFolderViewModel();

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
                Guid? organizationId = organizationManager.GetDefaultOrganization().Id;

                ServerFolder serverFolder = new ServerFolder()
                {
                    Id = id,
                    ParentFolderId = parentId,
                    CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
                    CreatedOn = DateTime.UtcNow,
                    Name = request.Name,
                    SizeInBytes = 0,
                    StorageDriveId = drive.Id,
                    StoragePath = path,
                    OrganizationId = organizationId
                };

                bool directoryExists = CheckDirectoryExists(shortPath);
                if (directoryExists)
                {
                    //create directory and add server folder
                    directoryManager.CreateDirectory(path);
                    serverFolderRepository.Add(serverFolder);
                    webhookPublisher.PublishAsync("Files.NewFolderCreated", serverFolder.Id.ToString(), serverFolder.Name);

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
            serverDriveRepository.Update(serverDrive);
            webhookPublisher.PublishAsync("Files.DriveUpdated", serverDrive.Id.ToString(), serverDrive.Name);
        }

        public void AddBytesToParentFolders(string path, long? size)
        {
            var pathArray = path.Split(Path.DirectorySeparatorChar);
            List<Guid?> parentIds = GetParentIds(pathArray);
            foreach (var serverFolderId in parentIds)
            {
                var folder = serverFolderRepository.Find(null).Items?.Where(q => q.Id == serverFolderId).FirstOrDefault();
                if (folder != null)
                {
                    folder.SizeInBytes += size;
                    serverFolderRepository.Update(folder);
                    webhookPublisher.PublishAsync("Files.FolderUpdated", folder.Id.ToString(), folder.Name);
                }
            }
        }

        public FileFolderViewModel SaveFile(FileFolderViewModel request, IFormFile file, ServerDrive drive)
        {
            Guid? id = Guid.NewGuid();
            string shortPath = request.StoragePath;
            string path = request.FullStoragePath;
            Guid? organizationId = organizationManager.GetDefaultOrganization().Id;
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
                CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                HashCode = hash,
                Name = file.FileName,
                SizeInBytes = file.Length,
                StorageFolderId = folderId,
                StoragePath = path,
                StorageProvider = Configuration["Files:StorageProvider"],
                OrganizationId = organizationId,
                ServerDriveId = drive.Id
            };
            serverFileRepository.Add(serverFile);
            webhookPublisher.PublishAsync("Files.NewFileCreated", serverFile.Id.ToString(), serverFile.Name);

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
                    CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
                    CreatedOn = DateTime.UtcNow,
                    DataType = attribute.Value.GetType().ToString(),
                    Name = attribute.Key,
                    OrganizationId = organizationId,
                    ServerDriveId = driveId
                };
                fileAttributeRepository.Add(fileAttribute);
                fileAttributes.Add(fileAttribute);
            }

            var viewModel = new FileFolderViewModel();
            viewModel = viewModel.Map(serverFile, shortPath);
            return viewModel;
        }

        //public void UpdateFile(UpdateServerFileViewModel request)
        //{
        //    Guid entityId = (Guid)request.Id;
        //    var file = request.File;
        //    string path = request.StoragePath;
        //    Guid? organizationId = organizationManager.GetDefaultOrganization().Id;
        //    var serverFile = serverFileRepository.GetOne(entityId);
        //    if (serverFile == null) throw new EntityDoesNotExistException("Server file could not be found");
        //    long? size = serverFile.SizeInBytes;
        //    var hash = GetHash(path);

        //    //update file attribute entities
        //    List<FileAttribute> fileAttributes = new List<FileAttribute>();
        //    var attributes = fileAttributeRepository.Find(null).Items?.Where(q => q.ServerFileId == entityId);
        //    if (attributes != null)
        //    {
        //        if (hash != serverFile.HashCode)
        //        {
        //            foreach (var attribute in attributes)
        //            {
        //                if (attribute.Name == FileAttributes.AppendCount.ToString() || attribute.Name == FileAttributes.StorageCount.ToString())
        //                {
        //                    attribute.AttributeValue += 1;

        //                    fileAttributeRepository.Update(attribute);
        //                }
        //                fileAttributes.Add(attribute);
        //            }
        //        }
        //    }
        //    else throw new EntityDoesNotExistException("File attribute entities could not be found for this file");

        //    //update server file entity properties
        //    serverFile.ContentType = file.ContentType;
        //    serverFile.HashCode = hash;
        //    serverFile.Name = file.FileName;
        //    serverFile.OrganizationId = organizationId;
        //    serverFile.SizeInBytes = file.Length;
        //    serverFile.StorageFolderId = request.StorageFolderId;
        //    serverFile.StoragePath = request.StoragePath;
        //    serverFile.StorageProvider = request.StorageProvider;
        //    serverFile.FileAttributes = fileAttributes;

        //    serverFileRepository.Update(serverFile);
        //    webhookPublisher.PublishAsync("Files.FileUpdated", serverFile.Id.ToString(), serverFile.Name);

        //    //update file stored in server
        //    bool directoryExists = CheckDirectoryExists(path);
        //    if (!directoryExists)
        //        throw new DirectoryNotFoundException("Storage path could not be found");

        //    path = Path.Combine(path, request.Id.ToString());

        //    if (file.Length > 0 && hash != serverFile.HashCode)
        //    {
        //        IOFile.Delete(path);
        //        using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
        //        {
        //            file.CopyTo(stream);
        //        }

        //        ConvertToBinaryObject(path);
        //    }

        //    //update size in bytes in server drive
        //    var drive = GetDriveById(serverFile.ServerDriveId);
        //    size = request.SizeInBytes - size;
        //    AddBytesToServerDrive(drive, size);
        //}

        public void DeleteFile(ServerFile serverFile)
        {
            //remove file attribute entities
            var attributes = fileAttributeRepository.Find(null).Items?.Where(q => q.ServerFileId == serverFile.Id);
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                    fileAttributeRepository.Delete((Guid)attribute.Id);
            }

            //remove file
            IOFile.Delete(serverFile.StoragePath);

            //remove server file entity
            serverFileRepository.SoftDelete((Guid)serverFile.Id);
            webhookPublisher.PublishAsync("Files.FileDeleted", serverFile.Id.ToString(), serverFile.Name);

            //update size in bytes in folder
            var size = -serverFile.SizeInBytes;
            AddBytesToParentFolders(serverFile.StoragePath, size);

            //update size in bytes in server drive
            var drive = GetDriveById(serverFile.ServerDriveId);
            AddBytesToServerDrive(drive, size);
        }

        protected enum FileAttributes
        {
            StorageCount,
            RetrievalCount,
            AppendCount
        }

        protected bool CheckDirectoryExists(string path)
        {
            if (directoryManager.Exists(path))
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

            Guid? driveId = GetDriveId(driveName);
            var file = serverFileRepository.Find(null).Items?.Where(q => q.Id.ToString() == id && q.ServerDriveId == driveId).FirstOrDefault();

            if (file != null)
            {
                var serverFolder = serverFolderRepository.Find(null).Items?.Where(q => q.Id == file.StorageFolderId).FirstOrDefault();
                Guid? folderId = Guid.Empty;
                string storagePath = string.Empty;
                if (serverFolder != null)
                {
                    folderId = serverFolder.Id;
                    storagePath = folder.StoragePath;
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
            }
            else
            {
                folder = serverFolderRepository.Find(null).Items?.Where(q => q.Id.ToString() == id && q.StorageDriveId == driveId).FirstOrDefault();

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
            ServerDrive drive = GetDriveByName(driveName);
            Guid? driveId;
            if (drive != null)
                driveId = drive.Id;
            else throw new EntityDoesNotExistException($"Drive {driveName} does not exist");

            return driveId;
        }

        public int? GetFileCount(string driveName)
        {
            Guid? driveId = GetDriveId(driveName);
            var files = serverFileRepository.Find(null).Items?.Where(q => q.ServerDriveId == driveId);
            int? count = files.Count();
            return count;
        }

        public int? GetFolderCount(string driveName)
        {
            Guid? driveId = GetDriveId(driveName);
            var folders = serverFolderRepository.Find(null).Items?.Where(q => q.StorageDriveId == driveId);
            int? count = folders.Count();
            return count;
        }

        public ServerFolder GetFolder(string name)
        {
            var serverFolder = serverFolderRepository.Find(null).Items?.Where(q => q.Name.ToLower() == name.ToLower()).FirstOrDefault();
            if (serverFolder == null)
                return null;
            return serverFolder;
        }

        public Guid? GetFolderId(string path, string driveName)
        {
            string[] pathArray = path.Split(Path.DirectorySeparatorChar);
            string folderName = pathArray[pathArray.Length - 1];
            var folder = GetFolder(folderName);
            Guid? folderId = folder?.Id;
            if (folderId == null)
            {
                var serverDrive = GetDriveByName(driveName);
                if (serverDrive != null)
                    folderId = serverDrive.Id;
                else throw new EntityDoesNotExistException("Drive could not be found");
            }

            return folderId;
        }

        public ServerDrive GetDriveById(Guid? id)
        {
            var serverDrive = serverDriveRepository.Find(null).Items?.Where(q => q.Id == id).FirstOrDefault();
            if (serverDrive == null)
                throw new EntityDoesNotExistException("Server drive could not be found");
            return serverDrive;
        }

        public ServerDrive GetDriveByName(string name)
        {
            var serverDrive = serverDriveRepository.Find(null).Items?.Where(q => q.Name == name).FirstOrDefault();
            if (serverDrive == null)
                throw new EntityDoesNotExistException("Server drive could not be found");
            return serverDrive;
        }

        public void DeleteFolder(ServerFolder folder)
        {
            //delete folder in directory
            directoryManager.Delete(folder.StoragePath);

            //delete folder in database
            serverFolderRepository.SoftDelete((Guid)folder.Id);
            webhookPublisher.PublishAsync("Files.FolderDeleted", folder.Id.ToString(), folder.Name);
        }

        public List<Guid?> GetParentIds(string[] pathArray)
        {
            List<Guid?> parentIds = new List<Guid?>();
            foreach (var folderName in pathArray)
            {
                var folder = serverFolderRepository.Find(null).Items?.Where(q => q.Name.ToLower() == folderName.ToLower()).FirstOrDefault();
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
            Guid? driveId = GetDriveId(driveName);
            var file = serverFileRepository.GetOne(entityId);
            var folder = serverFolderRepository.GetOne(entityId);
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
                    CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
                    CreatedOn = DateTime.UtcNow,
                    ExceptionJson = "",
                    ParametersJson = "",
                    ObjectId = file.Id,
                    MethodName = "Download",
                    ServiceName = ToString()
                };

                auditLogRepository.Add(auditLog);

                //export file
                fileFolder.StoragePath = file.StoragePath;
                fileFolder.Name = file.Name;
                fileFolder.ContentType = file.ContentType;
                fileFolder.Content = new FileStream(fileFolder?.StoragePath, FileMode.Open, FileAccess.Read);

                //update file attribute: retrieval count
                var retrievalFileAttribute = fileAttributeRepository.Find(null).Items?.Where(q => q.ServerFileId == file.Id && q.Name == FileAttributes.RetrievalCount.ToString()).FirstOrDefault();
                if (retrievalFileAttribute != null)
                {
                    retrievalFileAttribute.AttributeValue += 1;
                    fileAttributeRepository.Update(retrievalFileAttribute);
                }
            }
            else
                throw new EntityOperationException("Folders cannot be exported at this time");

            return fileFolder;
        }

        public bool CheckFolderHasChild(Guid? id)
        {
            bool hasChild = true;
            var folderChildren = serverFolderRepository.Find(null).Items?.Where(q => q.ParentFolderId == id);
            var fileChildren = serverFileRepository.Find(null).Items.Where(q => q.StorageFolderId == id);
            if (!folderChildren.Any() && !fileChildren.Any())
                hasChild = false;

            return hasChild;
        }

        public void DeleteFileFolder(string id, string driveName = null)
        {
            Guid? driveId = GetDriveId(driveName);
            var serverFile = serverFileRepository.Find(null).Items?.Where(q => q.Id.ToString() == id && q.ServerDriveId == driveId).FirstOrDefault();
            var serverFolder = new ServerFolder();
            if (serverFile != null)
            {
                DeleteFile(serverFile);
            }
            else if (serverFile == null)
            {
                serverFolder = serverFolderRepository.Find(null).Items?.Where(q => q.Id.ToString() == id && q.StorageDriveId == driveId).FirstOrDefault();
                if (serverFolder != null)
                {
                    bool hasChild = CheckFolderHasChild(serverFolder.Id);
                    if (serverFolder.SizeInBytes != 0)
                        throw new EntityOperationException("Folder cannot be deleted because it has files inside");
                    else if (hasChild)
                        throw new EntityOperationException("Folder cannot be deleted because it has folders inside");
                    DeleteFolder(serverFolder);
                }
                else
                    throw new EntityDoesNotExistException($"Folder with id '{id}' could not be found");
            }
            else
                throw new EntityDoesNotExistException($"File with id '{id}' could not be found");
        }

        public FileFolderViewModel RenameFileFolder(string id, string name, string driveName = null)
        {
            Guid? entityId = Guid.Parse(id);
            var fileFolder = new FileFolderViewModel();
            Guid? driveId = GetDriveId(driveName);
            var serverFile = serverFileRepository.Find(null).Items?.Where(q => q.Id == entityId && q.ServerDriveId == driveId).FirstOrDefault();
            var serverFolder = new ServerFolder();

            if (serverFile != null)
            {
                //rename file
                string oldPath = serverFile.StoragePath;
                var newNameArray = serverFile.Name.Split(".");
                var contentType = newNameArray[newNameArray.Length - 1];
                string newName = name + "." + contentType;
                string newPath = GetNewPath(serverFile.StoragePath, serverFile.Name, newName);
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
                serverFileRepository.Update(serverFile);

                IOFile.Move(oldPath, newPath);
                webhookPublisher.PublishAsync("Files.FileUpdated", id, serverFile.Name);

                fileFolder = fileFolder.Map(serverFile, shortPath);

                //update append file attribute
                var appendAttribute = fileAttributeRepository.Find(null).Items?.Where(q => q.Name == FileAttributes.AppendCount.ToString() && q.ServerFileId == serverFile.Id).FirstOrDefault();
                if (appendAttribute != null)
                    appendAttribute.AttributeValue += 1;
                fileAttributeRepository.Update(appendAttribute);
            }
            else if (serverFile == null)
            {
                serverFolder = serverFolderRepository.Find(null).Items?.Where(q => q.Id == entityId && q.StorageDriveId == driveId).FirstOrDefault(); 
                if (serverFolder != null)
                {
                    //rename folder
                    bool hasChild = CheckFolderHasChild(entityId);
                    string oldPath = serverFolder.StoragePath;
                    string oldName = serverFolder.Name;
                    string newPath = GetNewPath(serverFolder.StoragePath, oldName, name);
                    ExistingFolderCheck(newPath);
                    var newPathArray = newPath.Split(Path.DirectorySeparatorChar);
                    string shortPath = GetShortPath(newPath);

                    //move folder to new directory path
                    directoryManager.Move(oldPath, newPath);
                    serverFolder.Name = name;
                    serverFolder.StoragePath = newPath;
                    serverFolderRepository.Update(serverFolder);
                    webhookPublisher.PublishAsync("Files.FolderUpdated", id, serverFolder.Name);

                    //rename all files and folders underneath it
                    if (hasChild)
                        RenameChildFilesFolders(entityId, oldName, name);
                    fileFolder = fileFolder.Map(serverFolder, shortPath, hasChild);
                }
                else
                    throw new EntityDoesNotExistException($"Folder or file with id '{id}' could not be found");
            }

            return fileFolder;
        }

        public void RenameChildFilesFolders(Guid? entityId, string oldPath, string newPath)
        {
            var childFiles = serverFileRepository.Find(null).Items?.Where(q => q.StorageFolderId == entityId);
            if (childFiles.Any())
            {
                foreach (var file in childFiles)
                {
                    string oldFilePath = file.StoragePath;
                    string newFilePath = GetNewPath(file.StoragePath, oldPath, newPath);
                    file.StoragePath = newFilePath;
                    serverFileRepository.Update(file);
                    if (IOFile.Exists(oldPath))
                        IOFile.Move(oldFilePath, newFilePath);
                    webhookPublisher.PublishAsync("Files.FileUpdated", file.Id.ToString(), file.Name);
                }
            }
            var childFolders = serverFolderRepository.Find(null).Items?.Where(q => q.ParentFolderId == entityId);
            if (childFolders.Any())
            {
                foreach (var folder in childFolders)
                {
                    bool hasChild = CheckFolderHasChild(folder.Id);
                    string oldFolderPath = folder.StoragePath;
                    string newFolderPath = GetNewPath(folder.StoragePath, oldPath, newPath);
                    folder.StoragePath = newFolderPath;
                    serverFolderRepository.Update(folder);
                    if (directoryManager.Exists(oldPath))
                        directoryManager.Move(oldFolderPath, newFolderPath);
                    webhookPublisher.PublishAsync("Files.FolderUpdated", folder.Id.ToString(), folder.Name);
                    if (hasChild)
                        RenameChildFilesFolders(folder.Id, oldPath, newPath);
                }
            }
        }

        public string GetNewPath(string path, string oldPath, string newPath)
        {
            path = path.Replace(oldPath, newPath);
            return path;
        }

        public FileFolderViewModel MoveFileFolder(string fileFolderId, string parentFolderId, string driveName = null)
        {
            Guid? entityId = Guid.Parse(fileFolderId);
            Guid? parentId = Guid.Parse(parentFolderId);
            var fileFolder = new FileFolderViewModel();
            Guid? driveId = GetDriveId(driveName);
            var newParentFolder = serverFolderRepository.GetOne((Guid)parentId);
            string parentFolderPath = newParentFolder.StoragePath;

            var serverFile = serverFileRepository.Find(null).Items?.Where(q => q.Id == entityId && q.ServerDriveId == driveId).FirstOrDefault();
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
                serverFileRepository.Update(serverFile);

                IOFile.Move(oldPath, newPath);
                webhookPublisher.PublishAsync("Files.FileUpdated", fileFolderId, serverFile.Name);

                fileFolder = fileFolder.Map(serverFile, parentFolderPath);

                //update append file attribute
                var appendAttribute = fileAttributeRepository.Find(null).Items?.Where(q => q.Name == FileAttributes.AppendCount.ToString() && q.ServerFileId == serverFile.Id).FirstOrDefault();
                if (appendAttribute != null)
                    appendAttribute.AttributeValue += 1;
                fileAttributeRepository.Update(appendAttribute);

                //update new and old parent folder sizes
                UpdateFolderSize(oldParentFolderId, newParentFolder, serverFile);
            }
            else if (serverFile == null)
            {
                serverFolder = serverFolderRepository.Find(null).Items?.Where(q => q.Id == entityId && q.StorageDriveId == driveId).FirstOrDefault();           //else 
                if (serverFolder != null)
                {
                    //move folder
                    bool hasChild = CheckFolderHasChild(entityId);
                    Guid? oldParentFolderId = serverFolder.ParentFolderId;
                    string oldPath = serverFolder.StoragePath;
                    string newPath = Path.Combine(parentFolderPath, serverFolder.Name);
                    ExistingFolderCheck(newPath);

                    serverFolder.StoragePath = newPath;
                    serverFolder.ParentFolderId = parentId;
                    serverFolderRepository.Update(serverFolder);
                    webhookPublisher.PublishAsync("Files.FolderUpdated", serverFolder.Id.ToString(), serverFolder.Name);

                    //move folder to new directory path
                    directoryManager.Move(oldPath, newPath);
                    webhookPublisher.PublishAsync("Files.FolderUpdated", fileFolderId, serverFolder.Name);

                    //rename all files and folders underneath it
                    if (hasChild)
                        RenameChildFilesFolders(entityId, oldPath, newPath);
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
            var drive = GetDriveByName(driveName);
            Guid? driveId = drive.Id;
            var parentFolder = serverFolderRepository.GetOne((Guid)parentId);
            string parentFolderPath = parentFolder.StoragePath;

            var serverFile = serverFileRepository.Find(null).Items?.Where(q => q.Id == entityId && q.ServerDriveId == driveId).FirstOrDefault();
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
                    var newFile = serverFileRepository.GetOne((Guid)fileFolder.Id);

                    //add size in bytes of file to new parent folders and server drive
                    UpdateFolderSize(serverFile.StorageFolderId, parentFolder, newFile);

                    drive.StorageSizeInBytes += file.Length;
                    serverDriveRepository.Update(drive);
                }
            }
            else if (serverFile == null)
            {
                serverFolder = serverFolderRepository.Find(null).Items?.Where(q => q.Id == entityId && q.StorageDriveId == driveId).FirstOrDefault();           //else 
                if (serverFolder != null)
                {
                    string oldPath = serverFolder.StoragePath;
                    string newPath = Path.Combine(parentFolderPath, serverFolder.Name);
                    ExistingFolderCheck(newPath);

                    //create folder entity in database
                    serverFolder.ParentFolderId = parentId;
                    serverFolder.Id = Guid.NewGuid();
                    serverFolder.StoragePath = newPath;
                    serverFolder.CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name;
                    serverFolder.CreatedOn = DateTime.UtcNow;
                    serverFolder.Id = Guid.NewGuid();
                    serverFolder.UpdatedBy = null;
                    serverFolder.UpdatedOn = null;
                    serverFolderRepository.Add(serverFolder);

                    //map server folder to file folder view model
                    bool hasChild = CheckFolderHasChild(entityId);
                    fileFolder.Map(serverFolder, parentFolderPath, hasChild);

                    //create subdirectory structure in destination    
                    foreach (string dir in Directory.GetDirectories(oldPath, "*", SearchOption.AllDirectories))
                    {
                        //create copied folders with the same name
                        var folder = serverFolderRepository.Find(null).Items?.Where( q => q.StoragePath == dir).FirstOrDefault();
                        string shortPath = GetNewPath(folder.StoragePath, folder.StoragePath, serverFolder.StoragePath);
                        CheckDirectoryExists(shortPath);
                        string newFolderPath = Path.Combine(shortPath, folder.Name);
                        directoryManager.CreateDirectory(newFolderPath);

                        //add new folder entity in repository
                        var parentFolderEntity = serverFolderRepository.Find(null).Items?.Where(q => q.StoragePath == shortPath).FirstOrDefault();
                        folder.ParentFolderId = parentFolderEntity.Id;
                        folder.Id = Guid.NewGuid();
                        folder.StoragePath = Path.Combine(shortPath, folder.Name);
                        folder.CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name;
                        folder.CreatedOn = DateTime.UtcNow;
                        folder.Id = Guid.NewGuid();
                        folder.UpdatedBy = null;
                        folder.UpdatedOn = null;
                        serverFolderRepository.Add(folder);
                    }

                    foreach (string filePath in Directory.GetFiles(oldPath, "*", SearchOption.AllDirectories))
                    {
                        //copy all files to new directory
                        var file = serverFileRepository.Find(null).Items?.Where(q => q.StoragePath == filePath).FirstOrDefault();
                        //string newFilePath = GetNewPath(file.StoragePath, file.StoragePath, serverFolder.StoragePath);
                        string oldFilePath = file.StoragePath;
                        string newFilePath = GetNewPath(oldFilePath, oldPath, newPath);

                        //add new file entity in repository
                        string shortPath = GetShortPath(newFilePath);

                        using (var stream = IOFile.OpenRead(file.StoragePath))
                        {
                            IFormFile formFile = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

                            var parentFolderEntity = serverFolderRepository.Find(null).Items?.Where(q => q.StoragePath == shortPath).FirstOrDefault();
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
                    serverDriveRepository.Update(drive);
                }
                else
                    throw new EntityDoesNotExistException($"Folder or file with id '{fileFolderId}' could not be found");
            }

            return fileFolder;
        }

        public void ExistingFolderCheck(string path)
        {
            var folder = serverFolderRepository.Find(null).Items?.Where(q => q.StoragePath == path && q.IsDeleted == false).FirstOrDefault();
            if (folder != null)
                throw new EntityAlreadyExistsException($"Folder with path {path} already exists in current folder");
        }

        public void ExistingFileCheck(string path)
        {
            var file = serverFileRepository.Find(null).Items?.Where(q => q.StoragePath == path && q.IsDeleted == false).FirstOrDefault();
            if (file != null)
                throw new EntityAlreadyExistsException($"File with path {path} already exists in current folder");
        }

        public void UpdateFolderSize(Guid? oldParentFolderId, ServerFolder newParentFolder, ServerFile serverFile = null, ServerFolder serverFolder = null)
        {
            var oldParentFolder = serverFolderRepository.GetOne((Guid)oldParentFolderId);
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

        public FileFolderViewModel RenameFileFolder(string id, string name, string driveName = null)
        {
            var fileFolder = new FileFolderViewModel();
            return fileFolder;
        }
    }
}