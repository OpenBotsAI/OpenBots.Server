using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBots.Server.Business.File
{
    public class FileManager : BaseManager, IFileManager
    {
        private readonly LocalFileStorageAdapter _localFileStorageAdapter;

        public FileManager(
            LocalFileStorageAdapter localFileStorageAdapter)
        {
            _localFileStorageAdapter = localFileStorageAdapter;
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
                AddBytesToFoldersAndDrive(new List<FileFolderViewModel> { fileFolder });

            return fileFolder;
        }

        public void AddBytesToFoldersAndDrive(List<FileFolderViewModel> files)
        {
            string adapter = GetAdapterType("Files");
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                _localFileStorageAdapter.AddBytesToFoldersAndDrive(files);
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

        public StorageDrive AddStorageDrive(string driveName)
        {
            var storageDrive = new StorageDrive();
            string adapter = GetAdapterType(driveName);
            if (adapter.Equals(AdapterType.LocalFileStorage.ToString()))
                storageDrive = _localFileStorageAdapter.AddStorageDrive(driveName);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

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

        public string GetAdapterType(string driveName)
        {
            var drive = _localFileStorageAdapter.GetDriveByName(driveName);
            return drive.FileStorageAdapterType;
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
