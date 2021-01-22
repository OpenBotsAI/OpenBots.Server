using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
using OpenBots.Server.Web.Webhooks;
using Syncfusion.EJ2.FileManager.Base;
using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FileClass = System.IO.File;

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

        public PhysicalFileProvider operation;
        public string basePath;
        public string root = "Files";

        public IConfiguration Configuration { get; }

        public LocalFileStorageAdapter(
            IServerFileRepository serverFileRepository,
            IFileAttributeRepository fileAttributeRepository,
            IHttpContextAccessor httpContextAccessor,
            IOrganizationManager organizationManager,
            IServerFolderRepository serverFolderRepository,
            IServerDriveRepository serverDriveRepository,
            IWebHostEnvironment hostingEnvironment,
            IConfiguration configuration,
            IWebhookPublisher webhookPublisher)
        {
            this.fileAttributeRepository = fileAttributeRepository;
            this.serverFileRepository = serverFileRepository;
            this.httpContextAccessor = httpContextAccessor;
            this.organizationManager = organizationManager;
            this.serverFolderRepository = serverFolderRepository;
            this.serverDriveRepository = serverDriveRepository;
            this.webhookPublisher = webhookPublisher;
            Configuration = configuration;

            this.basePath = hostingEnvironment.ContentRootPath;
            this.operation = new PhysicalFileProvider();
            this.operation.RootFolder(this.basePath + "\\" + this.root);
        }

        public object LocalFileStorageOperation(FileManagerDirectoryContent args)
        {
            if (args.Action == "delete" || args.Action == "rename")
            {
                if ((args.TargetPath == null) && (args.Path == ""))
                {
                    FileManagerResponse response = new FileManagerResponse();
                    response.Error = new ErrorDetails { Code = "401", Message = "Restricted to modify the root folder." };
                    return this.operation.ToCamelCase(response);
                }
            }

            var serverDrive = GetDrive();
            var entityId = Guid.Parse(args.Id);
            ServerFile serverFile = new ServerFile();
            ServerFolder serverFolder = new ServerFolder();
            if (args.IsFile)
                serverFile = serverFileRepository.GetOne(entityId);
            else
                serverFolder = serverFolderRepository.GetOne(entityId);

            switch (args.Action)
            {
                case "read":
                    //reads the file(s) or folder(s) from the given path

                    //update FileAttribute (retrieval)
                    var fileAttribute = fileAttributeRepository.Find(null).Items?.Where(q => q.Name == FileAttributes.RetrievalCount.ToString()
                        && q.ServerFileId == entityId).FirstOrDefault();
                    fileAttribute.AttributeValue += 1;
                    fileAttributeRepository.Update(fileAttribute);

                    return this.operation.ToCamelCase(this.operation.GetFiles(args.Path, args.ShowHiddenItems));
                case "delete":
                    //deletes the selected file(s) or folder(s) from the given path
                    if (args.IsFile.Equals(true))
                    {
                        DeleteFile(args.Path);
                        serverDrive.StorageSizeInBytes -= args.Size;
        }
                    else
                    {
                        //removes the size of folder and any files from server drive property
                        var files = serverFileRepository.Find(null).Items?.Where(q => q.StorageFolderId == entityId);
                        long fileSize = 0;
                        foreach (var file in files)
                            fileSize += file.SizeInBytes;
                        serverDrive.StorageSizeInBytes -= args.Size - fileSize;

                        DeleteFolder(args.Path);
                    }

                    serverDriveRepository.Update(serverDrive);
                    webhookPublisher.PublishAsync("Files.DriveUpdated", serverDrive.Id.ToString(), serverDrive.Name);

                    return this.operation.ToCamelCase(this.operation.Delete(args.Path, args.Names));
                case "copy":
                    //copies the selected file(s) or folder(s) from a path and then pastes them into a given target path
                    if (args.IsFile.Equals(true))
                    {
                        foreach (var file in args.UploadFiles)
                        {
                            var folderId = GetFolderId(args.Path);

                            //add serverFile entity
                            SaveServerFileViewModel viewModel = new SaveServerFileViewModel()
                            {
                                Id = Guid.Parse(args.Id),
                                ContentType = file.ContentType,
                                CorrelationEntity = "", //TODO: update or remove
                                CorrelationEntityId = Guid.Empty, //TODO: update or remove
                                HashCode = null, 
                                SizeInBytes = file.Length,
                                StorageFolderId = folderId,
                                StoragePath = args.Path,
                                StorageProvider = "LocalFileStorage",
                                File = file
                            };
                            SaveFile(viewModel);
                        }
                    }
                    else
                    {
                        //add serverFolder entity
                        AddFolder(args);
                    }
                    AddBytesToServerDrive(serverDrive, args.Size);

                    return this.operation.ToCamelCase(this.operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
                case "move":
                    //cuts the selected file(s) or folder(s) from a path and then pastes them into a given target path
                    if (args.IsFile.Equals(true))
                    {
                        foreach (var file in args.UploadFiles)
                        {
                            var folderId = GetFolderId(args.Path);
                            UpdateServerFileViewModel viewModel = new UpdateServerFileViewModel()
                            {
                                ContentType = file.ContentType,
                                CorrelationEntity = "", //TODO: remove or update
                                CorrelationEntityId = Guid.Empty, //TODO: remove or update
                                HashCode = null,
                                File = file,
                                SizeInBytes = file.Length,
                                StorageFolderId = folderId,
                                StoragePath = Path.Combine(args.Path, file.FileName),
                                StorageProvider = "LocalFileStorage"
                            };
                            UpdateFile(viewModel);
                        }
                    }
                    else
                    {
                        //update ServerFolder entity
                        serverFolder.ParentFolderId = Guid.Parse(args.ParentId);
                        serverFolder.StoragePath = args.Path;
                        serverFolderRepository.Update(serverFolder);
                        webhookPublisher.PublishAsync("Files.FolderUpdated", serverFolder.Id.ToString(), serverFolder.Name);
                    }

                    return this.operation.ToCamelCase(this.operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
                case "details":
                    //gets the details of the selected file(s) or folder(s)
                    return this.operation.ToCamelCase(this.operation.Details(args.Path, args.Names, args.Data));
                case "create":
                    //creates a new folder in a given path

                    //add ServerFolder entity
                    AddFolder(args);
                    AddBytesToServerDrive(serverDrive, args.Size);

                    return this.operation.ToCamelCase(this.operation.Create(args.Path, args.Name));
                case "search":
                    //gets the list of file(s) or folder(s) from a given path based on the searched key string

                    //add to retrieval count value to file attribute entity for each file
                    //update FileAttribute (retrieval)
                    foreach (var file in args.UploadFiles)
                    {
                        fileAttribute = fileAttributeRepository.Find(null).Items?.Where(q => q.Name == FileAttributes.RetrievalCount.ToString()
                            && q.ServerFileId == entityId).FirstOrDefault();
                        fileAttribute.AttributeValue += 1;
                        fileAttributeRepository.Update(fileAttribute);
                    }

                    return this.operation.ToCamelCase(this.operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive));
                case "rename":
                    //renames a file or folder
                    if (args.IsFile.Equals(true))
                    {
                        //update ServerFile entity
                        foreach (var file in args.UploadFiles)
                        {
                            var folderId = GetFolderId(args.Path);
                            UpdateServerFileViewModel viewModel = new UpdateServerFileViewModel()
                            {
                                ContentType = file.ContentType,
                                CorrelationEntity = "", //TODO: remove or update
                                CorrelationEntityId = Guid.Empty, //TODO: remove or update
                                HashCode = null,
                                File = file,
                                SizeInBytes = file.Length,
                                StorageFolderId = folderId,
                                StoragePath = Path.Combine(args.Path, file.FileName),
                                StorageProvider = "LocalFileStorage"
                            };
                            UpdateFile(viewModel);
                        }
                    }
                    else
        {
                        //update ServerFolder entity
                        serverFolder.ParentFolderId = Guid.Parse(args.ParentId);
                        serverFolder.StoragePath = args.Path;
                        serverFolderRepository.Update(serverFolder);
                        webhookPublisher.PublishAsync("Files.FolderUpdated", serverFolder.Id.ToString(), serverFolder.Name);
                    }
                    return this.operation.ToCamelCase(this.operation.Rename(args.Path, args.Name, args.NewName));
            }
            return null;
        }

        public FileManagerResponse UploadFile(string path, IList<IFormFile> uploadFiles, string action)
            {
            long filesLength = 0;

            //add ServerFile entity for each file uploaded
            foreach (var file in uploadFiles)
            {
                path = Path.Combine(path, file.FileName);
                var folderId = GetFolderId(path);

                SaveServerFileViewModel viewModel = new SaveServerFileViewModel()
                {
                    Id = Guid.NewGuid(), //TODO: update to match args id
                    ContentType = file.ContentType,
                    CorrelationEntity = "", //TODO: update or remove from ServerFileViewModel and ServerFile
                    CorrelationEntityId = Guid.Empty, //TODO: update or remove from ServerFileViewModel and ServerFile
                    File = file,
                    HashCode = null,
                    StorageFolderId = folderId,
                    SizeInBytes = file.Length,
                    StoragePath = path,
                    StorageProvider = "LocalFileStorage"
                };

                SaveFile(viewModel);

                filesLength += file.Length;
            }

            //add to SizeInBytes property in ServerDrive
            var serverDrive = GetDrive();
            serverDrive.StorageSizeInBytes += filesLength;
            serverDriveRepository.Update(serverDrive);
            webhookPublisher.PublishAsync("Files.DriveUpdated", serverDrive.Id.ToString(), serverDrive.Name);

            FileManagerResponse uploadResponse = operation.Upload(path, uploadFiles, action, null);
            return uploadResponse;
        }

        public object DownloadFile(string downloadInput)
        {
            FileManagerDirectoryContent args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(downloadInput);

            //update FileAttribute (retrieval)
            var fileAttribute = fileAttributeRepository.Find(null).Items?.Where(q => q.Name == FileAttributes.RetrievalCount.ToString()
                && q.ServerFileId == Guid.Parse(args.Id)).FirstOrDefault();
            fileAttribute.AttributeValue += 1;
            fileAttributeRepository.Update(fileAttribute);

            return operation.Download(args.Path, args.Names, args.Data);
        }

        public void SaveFile(SaveServerFileViewModel request)
        {
            var file = request.File;
            Guid? id = request.Id;
            string path = request.StoragePath;
            Guid? organizationId = organizationManager.GetDefaultOrganization().Id;
            var hash = GetHash(path);

            //add FileAttribute entities
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
                    ServerFileId = request.Id,
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

            //add file properties to ServerFile entity
            var serverFile = new ServerFile()
            {
                Id = id,
                ContentType = file.ContentType,
                CorrelationEntity = request.CorrelationEntity,
                CorrelationEntityId = request.CorrelationEntityId,
                CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                HashCode = hash,
                Name = file.FileName,
                SizeInBytes = file.Length,
                StorageFolderId = request.StorageFolderId,
                StoragePath = path,
                StorageProvider = request.StorageProvider,
                OrganizationId = organizationId,
                FileAttributes = fileAttributes
            };
            serverFileRepository.Add(serverFile);
            webhookPublisher.PublishAsync("Files.NewFileCreated", serverFile.Id.ToString(), serverFile.Name);

            //upload file to local server
            //CheckDirectoryExists(path, organizationId);

            //if (file.Length <= 0 || file.Equals(null)) throw new Exception("No file exists");
            //if (file.Length > 0)
            //{
            //    path = Path.Combine(path, serverFile.Id.ToString());
            //    using (var stream = new FileStream(path, FileMode.Create))
            //        file.CopyTo(stream);

            //    ConvertToBinaryObject(path);
            //}
        }

        public void UpdateFile(UpdateServerFileViewModel request)
        {
            Guid entityId = (Guid)request.Id;
            var file = request.File;
            string path = request.StoragePath;
            Guid? organizationId = organizationManager.GetDefaultOrganization().Id;
            var serverFile = serverFileRepository.GetOne(entityId);
            if (serverFile == null) throw new EntityDoesNotExistException("Server file entity could not be found");
            var hash = GetHash(path);

            //update FileAttribute entities
            List<FileAttribute> fileAttributes = new List<FileAttribute>();
            var attributes = fileAttributeRepository.Find(null).Items?.Where(q => q.ServerFileId == entityId);
            if (attributes != null)
            {
                if (hash != serverFile.HashCode)
                {
                    foreach (var attribute in attributes)
                    {
                        attribute.AttributeValue += 1;

                        fileAttributeRepository.Update(attribute);
                        fileAttributes.Add(attribute);
                    }
                }
            }
            else throw new EntityDoesNotExistException("File attribute entities could not be found for this file");

            //update ServerFile entity properties
            serverFile.ContentType = file.ContentType;
            serverFile.CorrelationEntity = request.CorrelationEntity;
            serverFile.CorrelationEntityId = request.CorrelationEntityId;
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

            ////update file stored in Server
            //CheckDirectoryExists(path, organizationId);

            //path = Path.Combine(path, request.Id.ToString());

            //if (file.Length > 0 && hash != serverFile.HashCode)
            //{
            //    FileClass.Delete(path);
            //    using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            //    {
            //        file.CopyTo(stream);
            //    }

            //    ConvertToBinaryObject(path);
            //}
        }

        public void DeleteFile(string path)
        {
            //remove ServerFile entity
            var serverFile = serverFileRepository.Find(null).Items?.Where(q => q.StoragePath == path).FirstOrDefault();
            serverFileRepository.Delete((Guid)serverFile.Id);

            webhookPublisher.PublishAsync("Files.FileDeleted", serverFile.Id.ToString(), serverFile.Name);

            //remove FileAttribute entities
            var attributes = fileAttributeRepository.Find(null).Items?.Where(q => q.ServerFileId == serverFile.Id);
            foreach (var attribute in attributes)
                fileAttributeRepository.Delete((Guid)attribute.Id);

            ////remove file
            //if (directoryManager.Exists(path))
            //    directoryManager.Delete(path);
            //else throw new DirectoryNotFoundException("File path could not be found");
        }

        protected enum FileAttributes
        {
            StorageCount,
            RetrievalCount,
            AppendCount
        }

        //protected void CheckDirectoryExists(string path, Guid? organizationId)
        //{
        //    if (!directoryManager.Exists(path))
        //    {
        //        directoryManager.CreateDirectory(path);

        //        var pathArray = path.Split("/");
        //        var length = pathArray.Length;
        //        var storageDriveName = pathArray[0];
        //        var storageDriveId = serverDriveRepository.Find(null).Items?.Where(q => q.Name == storageDriveName).FirstOrDefault().Id;
        //        var parentFolderName = pathArray[length - 2];
        //        var parentFolderId = serverFolderRepository.Find(null).Items?.Where(q => q.Name == parentFolderName && q.OrganizationId == organizationId && q.StorageDriveId == storageDriveId).FirstOrDefault().Id;
        //        var serverFolder = new ServerFolder()
        //        {
        //            CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
        //            CreatedOn = DateTime.UtcNow,
        //            Name = pathArray[length - 1],
        //            OrganizationId = organizationId,
        //            ParentFolderId = parentFolderId,
        //            StorageDriveId = storageDriveId,
        //        };
        //        serverFolderRepository.Add(serverFolder);
        //        webhookPublisher.PublishAsync("Files.NewFolderCreated", serverFolder.Id.ToString(), serverFolder.Name);
        //    }
        //}

        protected string GetHash(string path)
        {
            string hash = string.Empty;
            byte[] bytes = FileClass.ReadAllBytes(path);
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
            byte[] bytes = FileClass.ReadAllBytes(filePath);
            FileClass.WriteAllBytes(filePath, bytes);
        }

        protected FileViewModel CreateFileViewModel(ServerFile serverFile, bool returnFile = false)
        {
            var file = new FileViewModel()
            {
                Name = serverFile.Name,
                ContentType = serverFile.ContentType,
                StoragePath = serverFile.StoragePath
            };
            if (returnFile == true)
                file.Content = new FileStream(serverFile?.StoragePath, FileMode.Open, FileAccess.Read);
            return file;
        }

        //public int? GetFolderCount()
        //{
        //    int? count = serverFolderRepository.Find(null).Items?.Count;
        //    return count;
        //}

        public ServerFolder GetFolder(string name)
        {
            var serverFolder = serverFolderRepository.Find(null).Items?.Where(q => q.Name.ToLower() == name.ToLower()).FirstOrDefault();
            return serverFolder;
        }

        public Guid? GetFolderId(string path)
        {
            string[] pathArray = path.Split("/");
            string folderName = pathArray[path.Length - 2];
            Guid? folderId = GetFolder(folderName).Id;
            return folderId;
        }

        public ServerDrive GetDrive()
        {
            var serverDrive = serverDriveRepository.Find(null).Items?.FirstOrDefault();
            return serverDrive;
        }

        public void DeleteFolder(string path)
        {
            ServerFolder folder = serverFolderRepository.Find(null).Items?.Where(q => q.StoragePath == path).FirstOrDefault();
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
        }

        public void AddFolder(FileManagerDirectoryContent args)
        {
            string path = Path.Combine(args.Path, args.NewName);
            var folderId = GetFolderId(path);
            var id = new Guid();

            ServerFolder serverFolder = new ServerFolder()
            {
                Id = id,
                ParentFolderId = folderId,
                CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                Name = args.Name,
                OrganizationId = organizationManager.GetDefaultOrganization().Id,
                SizeInBytes = args.Size,
                StorageDriveId = GetDrive().Id,
                StoragePath = path
            };

            serverFolderRepository.Add(serverFolder);
            webhookPublisher.PublishAsync("Files.NewFolderCreated", serverFolder.Id.ToString(), serverFolder.Name);
        }

        public object GetImage(FileManagerDirectoryContent args)
        {
            //update FileAttribute (retrieval)
            var fileAttribute = fileAttributeRepository.Find(null).Items?.Where(q => q.Name == FileAttributes.RetrievalCount.ToString()
                && q.ServerFileId == Guid.Parse(args.Id)).FirstOrDefault();
            fileAttribute.AttributeValue += 1;
            fileAttributeRepository.Update(fileAttribute);

            return this.operation.GetImage(args.Path, args.Id, false, null, null);
        }

        public void AddBytesToServerDrive(ServerDrive serverDrive, long size)
        {
            //add to SizeInBytes property in ServerDrive
            serverDrive.StorageSizeInBytes += size;
            serverDriveRepository.Update(serverDrive);
        }
    }
}