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

        public PaginatedList<FileFolderViewModel> GetFilesFolders(string driveId, bool? isFile = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100, string path = null)
        {
            string adapter = GetAdapterType(driveId);

            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                return _localFileStorageAdapter.GetFilesFolders(driveId, isFile, predicate, sortColumn, direction, skip, take, path);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

        }

        public int? GetFileCount(string driveId)
        {
            string adapter = GetAdapterType(driveId);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
            {
                int? count = _localFileStorageAdapter.GetFileCount(driveId);
                return count;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public int? GetFolderCount(string driveId)
        {
            string adapter = GetAdapterType(driveId);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
            {
                int? count = _localFileStorageAdapter.GetFolderCount(driveId);
                return count;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public FileFolderViewModel GetFileFolder(string id, string driveId, string type)
        {
            string adapter = GetAdapterType(driveId);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
            {
                var fileFolder = _localFileStorageAdapter.GetFileFolderViewModel(id, driveId, type);
                return fileFolder;
            }
            //else if (adapter.Equals(AdapterType.AzureBlobStorageAdapter.ToString()))
            //    content = azureBlobStorageAdapter.DownloadFile(downloadInput);
            //else if (adapter.Equals(AdapterType.AmazonEC2StorageAdapter.ToString()))
            //    content = amazonEC2StorageAdapter.DownloadFile(downloadInput);
            //else if (adapter.Equals(AdapterType.GoogleBlobStorageAdapter.ToString()))
            //    content = googleBlobStorageAdapter.DownloadFile(downloadInput);
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public StorageDrive GetDriveByName(string driveName = null)
        {
            string adapter = GetAdapterTypeByDriveName(driveName);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
            {
                var drive = _localFileStorageAdapter.GetDriveByName(driveName);
                return drive;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public StorageDrive GetDriveById(string driveId = null)
        {
            string adapter = GetAdapterType(driveId);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
            {
                Guid? id = Guid.Parse(driveId);
                var drive = _localFileStorageAdapter.GetDriveById(id);
                return drive;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public List<FileFolderViewModel> AddFileFolder(FileFolderViewModel request, string driveId)
        {
            var response = new List<FileFolderViewModel>();
            string adapter = GetAdapterType(driveId);

            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                response = _localFileStorageAdapter.AddFileFolder(request, driveId);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
        }

        public async Task<FileFolderViewModel> ExportFileFolder(string id, string driveId)
        {
            var response = new FileFolderViewModel();
            string adapter = GetAdapterType(driveId);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                response = await _localFileStorageAdapter.ExportFile(id, driveId);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
        }

        public FileFolderViewModel DeleteFileFolder(string id, string driveId, string type)
        {
            FileFolderViewModel fileFolder = new FileFolderViewModel();
            string adapter = GetAdapterType(driveId);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                fileFolder = _localFileStorageAdapter.DeleteFileFolder(id, driveId, type);
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
            string adapter = GetAdapterType(files[0].StorageDriveId.ToString());
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                _localFileStorageAdapter.AddBytesToFoldersAndDrive(files);
        }

        public void RemoveBytesFromFoldersAndDrive(List<FileFolderViewModel> files)
        {
            string adapter = GetAdapterType(files[0].StorageDriveId.ToString());
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                _localFileStorageAdapter.RemoveBytesFromFoldersAndDrive(files);
        }

        public FileFolderViewModel RenameFileFolder(string id, string name, string driveId, string type)
        {
            var response = new FileFolderViewModel();
            string adapter = GetAdapterType(driveId);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                response = _localFileStorageAdapter.RenameFileFolder(id, name, driveId, type);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
        }

        public FileFolderViewModel MoveFileFolder(string fileFolderId, string parentFolderId, string driveId, string type)
        {
            var response = new FileFolderViewModel();
            string adapter = GetAdapterType(driveId);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                response = _localFileStorageAdapter.MoveFileFolder(fileFolderId, parentFolderId, driveId, type);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
        }

        public FileFolderViewModel CopyFileFolder(string fileFolderId, string parentFolderId, string driveId, string type)
        {
            var response = new FileFolderViewModel();
            string adapter = GetAdapterType(driveId);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                response = _localFileStorageAdapter.CopyFileFolder(fileFolderId, parentFolderId, driveId, type);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
        }

        public FileFolderViewModel UpdateFile(FileFolderViewModel request)
        {
            string adapter = GetAdapterType(request.StorageDriveId.ToString());
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                return _localFileStorageAdapter.UpdateFile(request).Result;
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
            string adapter = GetAdapterTypeByDriveName(driveName);
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

            CheckDefaultDrive(drive, organizationId);

            var adapterType = drive.FileStorageAdapterType;
            if (string.IsNullOrEmpty(adapterType))
                adapterType = AdapterType.LocalFileStorage.ToString();

            //check if a new drive can be created for the current organization
            long? maxSizeInBytes = drive.MaxStorageAllowedInBytes;//size of new drive
            long? organizationStorage = GetTotalOrganizationStorage(organizationId);//sum of all drives for the current organization
            long? orgMaxSizeInBytes = _organizationManager.GetMaxStorageInBytes(organizationId);//max allowed storage for the current organization
            long? updatedOrgStorage = maxSizeInBytes + organizationStorage;//sum of new drive and all existing drives

            if (orgMaxSizeInBytes != null && maxSizeInBytes > orgMaxSizeInBytes)
            {
                throw new UnauthorizedOperationException("Drive size would exceed the allowed storage space for this organization", EntityOperationType.Add);
            }

            if (string.IsNullOrEmpty(drive.StoragePath))
                drive.StoragePath = drive.Name;
            if (drive.MaxStorageAllowedInBytes == null)
                drive.MaxStorageAllowedInBytes = orgMaxSizeInBytes;
            if (string.IsNullOrEmpty(drive.FileStorageAdapterType))
                drive.FileStorageAdapterType = AdapterType.LocalFileStorage.ToString();
            if (drive.IsDefault == null)
                drive.IsDefault = false;

            var storageDrive = new StorageDrive()
            {
                Name = drive.Name,
                FileStorageAdapterType = drive.FileStorageAdapterType,
                OrganizationId = organizationId,
                StoragePath = drive.StoragePath,
                CreatedBy = _httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                StorageSizeInBytes = drive.StorageSizeInBytes ?? 0,
                MaxStorageAllowedInBytes = drive.MaxStorageAllowedInBytes,
                IsDefault = drive.IsDefault
            };
            _storageDriveRepository.Add(storageDrive);

            if (adapterType == AdapterType.LocalFileStorage.ToString())
                _directoryManager.CreateDirectory(drive.Name);

            _webhookPublisher.PublishAsync("Files.NewDriveCreated", storageDrive.Id.ToString(), storageDrive.Name);

            return storageDrive;
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

        public string GetShortPath(string path)
        {
            return _localFileStorageAdapter.GetShortPath(path);
        }

        public void CheckDefaultDrive(StorageDrive drive, Guid? organizationId)
        {
            if (drive.IsDefault.Value)
            {
                var defaultDrive = _storageDriveRepository.Find(null).Items.Where(q => q.OrganizationId == organizationId && q.IsDefault == true).FirstOrDefault();
                if (defaultDrive != null && defaultDrive.Name != drive.Name)
                    throw new EntityOperationException($"Default drive {defaultDrive.Name} already exists");
            }
        }

        private string GetAdapterType(string driveId)
        {
            Guid id = Guid.Parse(driveId);
            var drive = _localFileStorageAdapter.GetDriveById(id);
            return drive.FileStorageAdapterType;
        }

        private string GetAdapterTypeByDriveName(string driveName)
        {
            var drive = _localFileStorageAdapter.GetDriveByName(driveName);
            return drive.FileStorageAdapterType;
        }

        private long? GetTotalOrganizationStorage(Guid? organizationId)
        {
            long? sum = 0;
            var organizationDrives = _storageDriveRepository.Find(null, d => d.OrganizationId == organizationId).Items;

            foreach (var drive in organizationDrives)
            {
                sum += drive.MaxStorageAllowedInBytes;
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
