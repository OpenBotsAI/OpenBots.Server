using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
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
        private readonly IDirectoryManager directoryManager;
        private readonly IOrganizationManager organizationManager;
        private readonly IServerFolderRepository serverFolderRepository;
        private readonly IServerDriveRepository serverDriveRepository;
        public IConfiguration Configuration { get; }

        public LocalFileStorageAdapter(
            IServerFileRepository serverFileRepository,
            IFileAttributeRepository fileAttributeRepository,
            IHttpContextAccessor httpContextAccessor,
            IDirectoryManager directoryManager,
            IOrganizationManager organizationManager,
            IServerFolderRepository serverFolderRepository,
            IServerDriveRepository serverDriveRepository,
            IConfiguration configuration)
        {
            this.fileAttributeRepository = fileAttributeRepository;
            this.serverFileRepository = serverFileRepository;
            this.httpContextAccessor = httpContextAccessor;
            this.directoryManager = directoryManager;
            this.organizationManager = organizationManager;
            this.serverFolderRepository = serverFolderRepository;
            this.serverDriveRepository = serverDriveRepository;
            Configuration = configuration;
        }

        public FileViewModel GetFile(string path)
        {
            var serverFile = serverFileRepository.Find(null).Items?.Where(q => q.StoragePath == path)?.FirstOrDefault();

            if (serverFile != null)
            {
                var file = CreateFileViewModel(serverFile, true);
                return file;
            }
            else throw new EntityDoesNotExistException($"File could not be found");
        }

        public FileViewModel SaveFile(SaveServerFileViewModel request)
        {
            var file = request.File;
            Guid? id = Guid.NewGuid();
            string path = request.StoragePath;
            Guid? organizationId = organizationManager.GetDefaultOrganization().Id;
            var hash = GetHash(path);

            //Add FileAttribute entities
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

            //Add file properties to ServerFile entity
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

            //Upload file to local Server
            CheckDirectoryExists(path, organizationId);

            if (file.Length <= 0 || file.Equals(null)) throw new Exception("No file exists");
            if (file.Length > 0)
            {
                path = Path.Combine(path, serverFile.Id.ToString());
                using (var stream = new FileStream(path, FileMode.Create))
                    file.CopyTo(stream);

                ConvertToBinaryObject(path);
            }

            var fileViewModel = CreateFileViewModel(serverFile);
            return fileViewModel;
        }

        public FileViewModel UpdateFile(UpdateServerFileViewModel request)
        {
            Guid entityId = (Guid)request.Id;
            var file = request.File;
            string path = request.StoragePath;
            Guid? organizationId = organizationManager.GetDefaultOrganization().Id;
            var serverFile = serverFileRepository.GetOne(entityId);
            if (serverFile == null) throw new EntityDoesNotExistException("Server file entity could not be found");
            var hash = GetHash(path);

            //Update FileAttribute entities
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

            //Update ServerFile entity properties
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


            //Update file stored in Server
            CheckDirectoryExists(path, organizationId);

            path = Path.Combine(path, request.Id.ToString());

            if (file.Length > 0 && hash != serverFile.HashCode)
            {
                FileClass.Delete(path);
                using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    file.CopyTo(stream);
                }

                ConvertToBinaryObject(path);
            }

            var fileViewModel = CreateFileViewModel(serverFile);
            return fileViewModel;
        }

        public void DeleteFile(string path)
        {
            //Remove ServerFile entity
            var serverFileId = serverFileRepository.Find(null).Items?.Where(q => q.StoragePath == path).FirstOrDefault().Id;
            serverFileRepository.Delete((Guid)serverFileId);

            //Remove FileAttribute entities
            var attributes = fileAttributeRepository.Find(null).Items?.Where(q => q.ServerFileId == serverFileId);
            foreach (var attribute in attributes)
                fileAttributeRepository.Delete((Guid)attribute.Id);

            //Remove file
            if (directoryManager.Exists(path))
                directoryManager.Delete(path);
            else throw new DirectoryNotFoundException("File path could not be found");
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

                var pathArray = path.Split("/");
                var length = pathArray.Length;
                var storageDriveName = pathArray[0];
                var storageDriveId = serverDriveRepository.Find(null).Items?.Where(q => q.Name == storageDriveName).FirstOrDefault().Id;
                var parentFolderName = pathArray[length - 2];
                var parentFolderId = serverFolderRepository.Find(null).Items?.Where(q => q.Name == parentFolderName && q.OrganizationId == organizationId && q.StorageDriveId == storageDriveId).FirstOrDefault().Id;
                var serverFolder = new ServerFolder()
                {
                    CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
                    CreatedOn = DateTime.UtcNow,
                    Name = pathArray[length - 1],
                    OrganizationId = organizationId,
                    ParentFolderId = parentFolderId,
                    StorageDriveId = storageDriveId,
                };
                serverFolderRepository.Add(serverFolder);
            }
        }

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

        public int? GetFolderCount()
        {
            int? count = serverFolderRepository.Find(null).Items?.Count;
            return count;
        }

        public ServerFolder GetFolder(string name)
        {
            var serverFolder = serverFolderRepository.Find(null).Items?.Where(q => q.Name == name).FirstOrDefault();
            return serverFolder;
        }

        public ServerDrive GetDrive()
        {
            var serverDrive = serverDriveRepository.Find(null).Items?.FirstOrDefault();
            return serverDrive;
        }
    }
}