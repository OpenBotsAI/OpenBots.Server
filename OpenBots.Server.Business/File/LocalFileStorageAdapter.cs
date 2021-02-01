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

        public PaginatedList<FileFolderViewModel> GetFilesFolders(bool? isFile = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            var filesFolders = new PaginatedList<FileFolderViewModel>();

            if (isFile.Equals(true))
            {
                //get all files with filters
                filesFolders = serverFileRepository.FindAllView(predicate, sortColumn, direction, skip, take);
            }
            else if (isFile.Equals(false))
            {
                //get all folders with filters
                filesFolders = serverFolderRepository.FindAllView(predicate, sortColumn, direction, skip, take);
            }
            else
            {
                //gets all files and folders with filters
                filesFolders = serverFolderRepository.FindAllView(predicate, sortColumn, direction, skip, take);
                int count = filesFolders.Items.Count;
                if (count < take)
                {
                    take -= count;
                    var files = serverFileRepository.FindAllView(predicate, sortColumn, direction, skip, take).Items;
                    if (files != null)
                    {
                        foreach (var file in files)
                            filesFolders.Add(file);
                    }
                    filesFolders.TotalCount += files.Count;
                }
            }

            return filesFolders;
        }

        public FileFolderViewModel SaveFile(FileFolderViewModel request)
        {
            IFormFile? file = request.File;
            Guid? id = Guid.NewGuid();
            string shortPath = request.StoragePath;
            string path = request.FullStoragePath;
            Guid? organizationId = organizationManager.GetDefaultOrganization().Id;

            var checkFile = serverFileRepository.Find(null)?.Items?.Where(q => q.StoragePath == path).FirstOrDefault();
            if (checkFile != null)
                throw new EntityAlreadyExistsException($"File with name {request.Name} already exists at path {request.StoragePath}");

            //upload file to local server
            CheckDirectoryExists(shortPath, organizationId);

            if (file.Length <= 0 || file.Equals(null)) throw new Exception("No file exists");
            if (file.Length > 0)
            {
                using (var stream = new FileStream(path, FileMode.Create))
                    file.CopyTo(stream);

                ConvertToBinaryObject(path);
            }

            Guid? folderId = GetFolderId(path);
            var hash = GetHash(path);
            var drive = GetDriveByName(path);
            Guid? driveId = Guid.Empty;
            if (drive != null)
                driveId = drive.Id;
            else throw new EntityDoesNotExistException("Drive could not be found");

            //add file properties to server file entity
            var serverFile = new ServerFile()
            {
                Id = id,
                ContentType = file.ContentType,
                CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                HashCode = hash,
                Name = file.FileName,
                SizeInBytes = file.Length,
                StorageFolderId = folderId,
                StoragePath = path,
                StorageProvider = Configuration["Files:StorageProvider"],
                OrganizationId = organizationId,
                ServerDriveId = driveId
            };
            serverFileRepository.Add(serverFile);
            webhookPublisher.PublishAsync("Files.NewFileCreated", serverFile.Id.ToString(), serverFile.Name);

            //add size in bytes to server drive
            AddBytesToServerDrive(drive, serverFile.SizeInBytes);

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
                    OrganizationId = organizationId
                };
                fileAttributeRepository.Add(fileAttribute);
                fileAttributes.Add(fileAttribute);
            }

            var viewModel = new FileFolderViewModel();
            viewModel = viewModel.Map(serverFile, shortPath);
            return viewModel;
        }

        public void UpdateFile(UpdateServerFileViewModel request)
        {
            Guid entityId = (Guid)request.Id;
            var file = request.File;
            string path = request.StoragePath;
            Guid? organizationId = organizationManager.GetDefaultOrganization().Id;
            var serverFile = serverFileRepository.GetOne(entityId);
            if (serverFile == null) throw new EntityDoesNotExistException("Server file could not be found");
            long? size = serverFile.SizeInBytes;
            var hash = GetHash(path);

            //update file attribute entities
            List<FileAttribute> fileAttributes = new List<FileAttribute>();
            var attributes = fileAttributeRepository.Find(null).Items?.Where(q => q.ServerFileId == entityId);
            if (attributes != null)
            {
                if (hash != serverFile.HashCode)
                {
                    foreach (var attribute in attributes)
                    {
                        if (attribute.Name == FileAttributes.AppendCount.ToString() || attribute.Name == FileAttributes.StorageCount.ToString())
                        {
                            attribute.AttributeValue += 1;

                            fileAttributeRepository.Update(attribute);
                        }
                        fileAttributes.Add(attribute);
                    }
                }
            }
            else throw new EntityDoesNotExistException("File attribute entities could not be found for this file");

            //update server file entity properties
            serverFile.ContentType = file.ContentType;
            serverFile.HashCode = hash;
            serverFile.Name = file.FileName;
            serverFile.OrganizationId = organizationId;
            serverFile.SizeInBytes = file.Length;
            serverFile.StorageFolderId = request.StorageFolderId;
            serverFile.StoragePath = request.StoragePath;
            serverFile.StorageProvider = request.StorageProvider;
            serverFile.FileAttributes = fileAttributes;

            serverFileRepository.Update(serverFile);
            webhookPublisher.PublishAsync("Files.FileUpdated", serverFile.Id.ToString(), serverFile.Name);

            //update file stored in server
            CheckDirectoryExists(path, organizationId);

            path = Path.Combine(path, request.Id.ToString());

            if (file.Length > 0 && hash != serverFile.HashCode)
            {
                IOFile.Delete(path);
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    file.CopyTo(stream);
                }

                ConvertToBinaryObject(path);
            }

            //update size in bytes in server drive
            var drive = GetDriveById(serverFile.ServerDriveId);
            size = request.SizeInBytes - size;
            AddBytesToServerDrive(drive, size);
        }

        public void DeleteFile(string path)
        {
            //remove server file entity
            var serverFile = serverFileRepository.Find(null).Items?.Where(q => q.StoragePath == path).FirstOrDefault();
            serverFileRepository.Delete((Guid)serverFile.Id);

            webhookPublisher.PublishAsync("Files.FileDeleted", serverFile.Id.ToString(), serverFile.Name);

            //remove file attribute entities
            var attributes = fileAttributeRepository.Find(null).Items?.Where(q => q.ServerFileId == serverFile.Id);
            foreach (var attribute in attributes)
                fileAttributeRepository.Delete((Guid)attribute.Id);

            //remove file
            if (directoryManager.Exists(path))
                directoryManager.Delete(path);
            else throw new DirectoryNotFoundException("File path could not be found");

            //update size in bytes in server drive
            var drive = GetDriveById(serverFile.ServerDriveId);
            var size = -serverFile.SizeInBytes;
            AddBytesToServerDrive(drive, size);
        }

        protected enum FileAttributes
        {
            StorageCount,
            RetrievalCount,
            AppendCount
        }

        protected void CheckDirectoryExists(string path, Guid? organizationId)
        {
            if (!directoryManager.Exists(path))
            {
                directoryManager.CreateDirectory(path);

                var pathArray = path.Split("\\");
                var length = pathArray.Length;
                var serverDrive = GetDriveByName(path);
                Guid? serverDriveId = Guid.Empty;
                if (serverDrive != null)
                    serverDriveId = serverDrive.Id;
                else
                    throw new EntityDoesNotExistException("Drive could not be found");
                var parentFolderName = pathArray[length - 2];
                var parentFolderId = serverFolderRepository.Find(null).Items?.Where(q => q.Name == parentFolderName && q.OrganizationId == organizationId && q.StorageDriveId == serverDriveId).FirstOrDefault().Id;
                var serverFolder = new ServerFolder()
                {
                    CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
                    CreatedOn = DateTime.UtcNow,
                    Name = pathArray[length - 1],
                    OrganizationId = organizationId,
                    ParentFolderId = parentFolderId,
                    StorageDriveId = serverDriveId,
                };
                serverFolderRepository.Add(serverFolder);
                webhookPublisher.PublishAsync("Files.NewFolderCreated", serverFolder.Id.ToString(), serverFolder.Name);
            }
        }

        protected string GetHash(string path)
        {
            string hash = string.Empty;
            byte[] bytes = IOFile.ReadAllBytes(path);
            using (SHA256 sha256Hash = SHA256.Create())
            {
                //hash = GetHashCode(sha256Hash, bytes);
                HashAlgorithm hashAlgorithm = sha256Hash;
                byte[] data = hashAlgorithm.ComputeHash(bytes);
                var sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                    sBuilder.Append(data[i].ToString("x2"));
                hash = sBuilder.ToString();
            }
            return hash;
        }

        //protected string GetHashCode(HashAlgorithm hashAlgorithm, byte[] input)
        //{
        //    //Convert the input string to a byte array and compute the hash
        //    byte[] data = hashAlgorithm.ComputeHash(input);
        //    //Create new StringBuilder to collect the bytes and create a string
        //    var sBuilder = new StringBuilder();
        //    //Loop through each byte of the hashed data and format each one as a hexidecimal string
        //    for (int i = 0; i < data.Length; i++)
        //        sBuilder.Append(data[i].ToString("x2"));
        //    //Return the hexidecimal string
        //    return sBuilder.ToString();
        //}

        protected void ConvertToBinaryObject(string filePath)
        {
            byte[] bytes = IOFile.ReadAllBytes(filePath);
            IOFile.WriteAllBytes(filePath, bytes);
        }

        public FileFolderViewModel GetFileFolderViewModel(string id, bool returnFile = false)
        {
            ServerFolder folder = new ServerFolder();
            var fileFolder = new FileFolderViewModel();
            var file = serverFileRepository.Find(null).Items?.Where(q => q.Id.ToString() == id).FirstOrDefault();

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

                if (returnFile == true)
                    fileFolder.Content = new FileStream(file?.StoragePath, FileMode.Open, FileAccess.Read);
            }
            else
            {
                folder = serverFolderRepository.Find(null).Items?.Where(q => q.Id.ToString() == id).FirstOrDefault();

                if (folder == null)
                    throw new EntityDoesNotExistException($"File or folder does not exist");

                var pathArray = folder.StoragePath.Split("\\");
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
                fileFolder.StoragePath = string.Join("\\", shortPathArray);
                fileFolder.CreatedBy = folder.CreatedBy;
                fileFolder.CreatedOn = folder.CreatedOn;
                fileFolder.FullStoragePath = folder.StoragePath;
                fileFolder.Size = folder.SizeInBytes;
                fileFolder.HasChild = hasChild;
                fileFolder.IsFile = false;
                fileFolder.ParentId = folder.ParentFolderId;
            }

            return fileFolder;
        }

        public int? GetFolderCount()
        {
            int? count = serverFolderRepository.Find(null).Items?.Count;
            return count;
        }

        public ServerFolder GetFolder(string name)
        {
            var serverFolder = serverFolderRepository.Find(null).Items?.Where(q => q.Name.ToLower() == name.ToLower()).FirstOrDefault();
            if (serverFolder == null)
                return null;
            return serverFolder;
        }

        public Guid? GetFolderId(string path)
        {
            string[] pathArray = path.Split("\\");
            string folderName = pathArray[pathArray.Length - 2];
            var folder = GetFolder(folderName);
            Guid? folderId = folder?.Id;
            if (folderId == null)
            {
                var serverDrive = GetDriveByName(path);
                if (serverDrive != null)
                    folderId = serverDrive.Id;
                else throw new EntityDoesNotExistException("Drive for storage path could not be found");
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

        public ServerDrive GetDriveByName(string path)
        {
            string name = path.Split("\\")[0];
            var serverDrive = serverDriveRepository.Find(null).Items?.Where(q => q.Name == name).FirstOrDefault();

            return serverDrive;
        }

        public void DeleteFolder(string path)
        {
            ServerFolder folder = serverFolderRepository.Find(null).Items?.Where(q => q.StoragePath == path).FirstOrDefault();

            if (folder == null)
                throw new EntityDoesNotExistException($"Folder with path {path} could not be found");

            serverFolderRepository.Delete((Guid)folder.Id);
            webhookPublisher.PublishAsync("Files.FolderDeleted", folder.Id.ToString(), folder.Name);

            //delete any files that are in the folder
            var files = serverFileRepository.Find(null).Items?.Where(q => q.StorageFolderId == folder.Id);
            if (files != null)
            {
                foreach (var file in files)
                {
                    DeleteFile(file.StoragePath);
                    webhookPublisher.PublishAsync("Files.FileDeleted", file.Id.ToString(), file.Name);
                }
            }

            //update size in bytes in server drive
            var drive = GetDriveById(folder.StorageDriveId);
            var size = -folder.SizeInBytes;
            AddBytesToServerDrive(drive, size);
        }

        public FileFolderViewModel AddFileFolder(FileFolderViewModel request)
        {
            var newFileFolder = new FileFolderViewModel();
            if ((bool)request.IsFile)
            {
                if (request.File == null)
                    throw new EntityOperationException("No file uploaded");

                long size = request.File.Length;
                if (size <= 0)
                    throw new EntityOperationException($"File size of file {request.File.FileName} cannot be 0");

                //add file
                request.FullStoragePath = Path.Combine(request.StoragePath, request.File.FileName);
                newFileFolder = SaveFile(request);
            }
            else
            {
                //add folder
                string path = Path.Combine(request.StoragePath, request.Name);
                request.FullStoragePath = path;
                var folderId = GetFolderId(path);
                var id = Guid.NewGuid();

                var folder = serverFolderRepository.Find(null).Items?.Where(q => q.StoragePath == request.FullStoragePath).FirstOrDefault();
                if (folder != null)
                    throw new EntityAlreadyExistsException($"Folder with name {request.Name} already exists at path {request.StoragePath}");

                Guid? serverDriveId = Guid.Empty;
                var serverDrive = GetDriveByName(path);
                if (serverDrive != null)
                    serverDriveId = serverDrive.Id;
                if (serverDriveId == Guid.Empty)
                    throw new EntityDoesNotExistException("Drive for storage path could not be found");

                Guid? organizationId = organizationManager.GetDefaultOrganization().Id;

                ServerFolder serverFolder = new ServerFolder()
                {
                    Id = id,
                    ParentFolderId = folderId,
                    CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
                    CreatedOn = DateTime.UtcNow,
                    Name = request.Name,
                    SizeInBytes = 0,
                    StorageDriveId = serverDriveId,
                    StoragePath = path,
                    OrganizationId = organizationId
                };

                CheckDirectoryExists(path, organizationId);
                serverFolderRepository.Add(serverFolder);
                webhookPublisher.PublishAsync("Files.NewFolderCreated", serverFolder.Id.ToString(), serverFolder.Name);

                var hasChild = CheckFolderHasChild(serverFolder.Id);
                newFileFolder = newFileFolder.Map(serverFolder, request.StoragePath, hasChild);
            }
            return newFileFolder;
        }

        public void AddBytesToServerDrive(ServerDrive serverDrive, long? size)
        {
            //add to storage size in bytes property in server drive
            serverDrive.StorageSizeInBytes += size;
            serverDriveRepository.Update(serverDrive);
            webhookPublisher.PublishAsync("Files.DriveUpdated", serverDrive.Id.ToString(), serverDrive.Name);
        }

        //public List<Guid?> GetParentIds(string[] pathArray)
        //{
        //    List<Guid?> parentIds = new List<Guid?>();
        //    foreach (var folderName in pathArray)
        //    {
        //        var folder = serverFolderRepository.Find(null).Items?.Where(q => q.Name.ToLower() == folderName.ToLower()).FirstOrDefault();
        //        Guid? folderId = folder?.Id;
        //        Guid? driveId = GetDrive().Id;
        //        if (folderName == "Files")
        //            folderId = driveId;
        //        if (folderId != null)
        //            parentIds.Add(folderId);
        //    }

        //    return parentIds;
        //}

        public async Task<FileFolderViewModel> ExportFile(string id)
        {
            Guid entityId = Guid.Parse(id);
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
            var children = serverFolderRepository.Find(null).Items?.Where(q => q.ParentFolderId == id);
           if (!children.Any())
                hasChild = false;

            return hasChild;
        }
    }
}