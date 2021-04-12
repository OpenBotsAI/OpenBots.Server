using Microsoft.AspNetCore.Http;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
using OpenBots.Server.Web.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Business.File
{
    public class FileManager : BaseManager, IFileManager
    {
        private readonly ILocalFileStorageAdapter _localFileStorageAdapter;
        private readonly IStorageDriveRepository _storageDriveRepository;
        private readonly IOrganizationManager _organizationManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebhookPublisher _webhookPublisher;
        private readonly IDirectoryManager _directoryManager;

        public FileManager(
            ILocalFileStorageAdapter localFileStorageAdapter,
            IStorageDriveRepository storageDriveRepository,
            IOrganizationManager organizationManager,
            IHttpContextAccessor httpContextAccessor,
            IWebhookPublisher webhookPublisher,
            IDirectoryManager directoryManager
)
        {
            _localFileStorageAdapter = localFileStorageAdapter;
            _storageDriveRepository = storageDriveRepository;
            _organizationManager = organizationManager;
            _httpContextAccessor = httpContextAccessor;
            _webhookPublisher = webhookPublisher;
            _directoryManager = directoryManager;
        }

        public PaginatedList<FileFolderViewModel> GetFilesFolders(bool? isFile = null, string driveName = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            string adapter = GetAdapterType(driveName);

            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                return _localFileStorageAdapter.GetFilesFolders(isFile, driveName, predicate, sortColumn, direction, skip, take);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

        }

        public int? GetFileCount(string driveName)
        {
            string adapter = GetAdapterType(driveName);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
            {
                int? count = _localFileStorageAdapter.GetFileCount(driveName);
                return count;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public int? GetFolderCount(string driveName)
        {
            string adapter = GetAdapterType(driveName);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
            {
                int? count = _localFileStorageAdapter.GetFolderCount(driveName);
                return count;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public FileFolderViewModel GetFileFolder(string id, string driveName = null)
        {
            string adapter = GetAdapterType(driveName);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
            {
                var folder = _localFileStorageAdapter.GetFileFolderViewModel(id, driveName);
                return folder;
            }
            //else if (adapter.Equals(AdapterType.AzureBlobStorageAdapter.ToString()))
            //    content = azureBlobStorageAdapter.DownloadFile(downloadInput);
            //else if (adapter.Equals(AdapterType.AmazonEC2StorageAdapter.ToString()))
            //    content = amazonEC2StorageAdapter.DownloadFile(downloadInput);
            //else if (adapter.Equals(AdapterType.GoogleBlobStorageAdapter.ToString()))
            //    content = googleBlobStorageAdapter.DownloadFile(downloadInput);
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public StorageDrive GetDrive(string driveName = null)
        {
            string adapter = GetAdapterType(driveName);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
            {
                var drive = _localFileStorageAdapter.GetDriveByName(driveName);
                return drive;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public List<FileFolderViewModel> AddFileFolder(FileFolderViewModel request, string driveName = null)
        {
            var response = new List<FileFolderViewModel>();
            string adapter = GetAdapterType(driveName);

            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                response = _localFileStorageAdapter.AddFileFolder(request, driveName);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
        }

        public async Task<FileFolderViewModel> ExportFileFolder(string id, string driveName = null)
        {
            var response = new FileFolderViewModel();
            string adapter = GetAdapterType(driveName);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                response = await _localFileStorageAdapter.ExportFile(id, driveName);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
        }

        public FileFolderViewModel DeleteFileFolder(string id, string driveName = null)
        {
            FileFolderViewModel fileFolder = new FileFolderViewModel();
            string adapter = GetAdapterType(driveName);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                fileFolder = _localFileStorageAdapter.DeleteFileFolder(id, driveName);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            if (fileFolder.Size > 0 && (fileFolder.IsFile == true
                && !fileFolder.StoragePath.Contains("Queue Item Attachments") && !fileFolder.StoragePath.Contains("Email Attachments")
                && !fileFolder.StoragePath.Contains("Automations") && !fileFolder.StoragePath.Contains("Assets") 
                || fileFolder.IsFile == false && (fileFolder.StoragePath.Contains("Automations") || fileFolder.StoragePath.Contains("Assets")
                || fileFolder.StoragePath.Contains("Email Attachments") || fileFolder.StoragePath.Contains("Queue Item Attachments"))))
                RemoveBytesFromFoldersAndDrive(new List<FileFolderViewModel> { fileFolder });

            return fileFolder;
        }

        public void AddBytesToFoldersAndDrive(List<FileFolderViewModel> files)
        {
            string adapter = GetAdapterType("Files");
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                _localFileStorageAdapter.AddBytesToFoldersAndDrive(files);
        }

        public void RemoveBytesFromFoldersAndDrive(List<FileFolderViewModel> files)
        {
            string adapter = GetAdapterType("Files");
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                _localFileStorageAdapter.RemoveBytesFromFoldersAndDrive(files);
        }

        public FileFolderViewModel RenameFileFolder(string id, string name, string driveName = null)
        {
            var response = new FileFolderViewModel();
            string adapter = GetAdapterType(driveName);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                response = _localFileStorageAdapter.RenameFileFolder(id, name, driveName);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
        }

        public FileFolderViewModel MoveFileFolder(string fileFolderId, string parentFolderId, string driveName = null)
        {
            var response = new FileFolderViewModel();
            string adapter = GetAdapterType(driveName);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                response = _localFileStorageAdapter.MoveFileFolder(fileFolderId, parentFolderId, driveName);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
        }

        public FileFolderViewModel CopyFileFolder(string fileFolderId, string parentFolderId, string driveName = null)
        {
            var response = new FileFolderViewModel();
            string adapter = GetAdapterType(driveName);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                response = _localFileStorageAdapter.CopyFileFolder(fileFolderId, parentFolderId, driveName);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
        }

        public void UpdateFile(FileFolderViewModel request)
        {
            string adapter = GetAdapterType("Files");
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                _localFileStorageAdapter.UpdateFile(request);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }


        public FileFolderViewModel GetFileFolderByStoragePath(string storagePath, string driveName = null)
        {
            var response = new FileFolderViewModel();
            string adapter = GetAdapterType(driveName);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                response = _localFileStorageAdapter.GetFileFolderByStoragePath(storagePath, driveName);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
        }

        public StorageDrive AddStorageDrive(StorageDrive drive)
        {
            Guid? organizationId = _organizationManager.GetDefaultOrganization().Id;
            var existingDrive = _storageDriveRepository.Find(null).Items?.Where(q => q.OrganizationId == organizationId && q.Name.ToLower() == drive.Name.ToLower()).FirstOrDefault();
            if (existingDrive != null)
                throw new EntityAlreadyExistsException($"Drive {drive.Name} already exists within this organization");

            var adapterType = drive.FileStorageAdapterType;
            if (string.IsNullOrEmpty(adapterType))
                adapterType = AdapterType.LocalFileStorage.ToString();

            //check if a new drive can be created for the current organization
            long? maxSizeInBytes = drive.MaxStorageAllowedInBytes;//size of new drive
            long? organizationStorage = GetTotalOrganizationStorage(organizationId);//sum of all drives for the current organization
            long? orgMaxSizeInBytes = _organizationManager.GetMaxStorageInBytes();//max allowed storage for the current organization
            long? updatedOrgStorage = maxSizeInBytes + organizationStorage;//sum of new drive and all existing drives

            if (orgMaxSizeInBytes != null && maxSizeInBytes > orgMaxSizeInBytes)
            {
                throw new UnauthorizedOperationException("Drive size would exceed the allowed storage space for this organization", EntityOperationType.Add);
            }

            var serverDrive = new StorageDrive()
            {
                FileStorageAdapterType = drive.FileStorageAdapterType,
                OrganizationId = organizationId,
                StoragePath = drive.StoragePath,
                CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                StorageSizeInBytes = drive.StorageSizeInBytes,
                MaxStorageAllowedInBytes = drive.MaxStorageAllowedInBytes
            };
            _storageDriveRepository.Add(serverDrive);

            if (adapterType == AdapterType.LocalFileStorage.ToString())
                _directoryManager.CreateDirectory(drive.Name);

            _webhookPublisher.PublishAsync("Files.NewDriveCreated", serverDrive.Id.ToString(), serverDrive.Name);

            return serverDrive;
        }

        public Dictionary<Guid?, string> GetDriveNames(string adapterType)
        {
            var driveNames = new Dictionary<Guid?, string>();
            string adapter = adapterType;
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                driveNames = _localFileStorageAdapter.GetDriveNames(adapterType);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return driveNames;
        }

        public string GetAdapterType(string driveName)
        {
            var drive = _localFileStorageAdapter.GetDriveByName(driveName);
            return drive.FileStorageAdapterType;
        }

        public string GetShortPath(string path)
        {
            return _localFileStorageAdapter.GetShortPath(path);
        }

        private long? GetTotalOrganizationStorage(Guid? organizationId)
        {
            long? sum = 0;
            var organizationDrives = _storageDriveRepository.Find(null, d => d.OrganizationId == organizationId).Items;

            foreach (var drive in organizationDrives)
            {
                sum += drive.StorageSizeInBytes;
            }
            return sum;
        }

        public enum AdapterType
        {
            LocalFileStorage,
            AzureBlobStorage,
            AmazonEC2Storage,
            GoogleBlobStorage
        }
    }
}
