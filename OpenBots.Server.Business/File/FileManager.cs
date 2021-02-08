using Microsoft.Extensions.Configuration;
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

        public IConfiguration Configuration { get; }

        public FileManager(
            IConfiguration configuration,
            LocalFileStorageAdapter localFileStorageAdapter)
        {
            Configuration = configuration;
            _localFileStorageAdapter = localFileStorageAdapter;
        }

        public PaginatedList<FileFolderViewModel> GetFilesFolders(bool? isFile = null, string driveName = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            string adapter = Configuration["Files:Adapter"];

            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
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
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
            {
                int? count = _localFileStorageAdapter.GetFileCount(driveName);
                return count;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public int? GetFolderCount(string driveName)
        {
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
            {
                int? count = _localFileStorageAdapter.GetFolderCount(driveName);
                return count;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public FileFolderViewModel GetFileFolder(string id, string driveName = null)
        {
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
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

        public ServerDrive GetDrive(string driveName = null)
        {
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
            {
                var drive = _localFileStorageAdapter.GetDriveByName(driveName);
                return drive;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public List<FileFolderViewModel> AddFileFolder(FileFolderViewModel request, string driveName = null)
        {
            var response = new List<FileFolderViewModel>();
            string adapter = Configuration["Files:Adapter"];

            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
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
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
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

        public void DeleteFileFolder(string id, string driveName = null)
        {
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
                _localFileStorageAdapter.DeleteFileFolder(id, driveName);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public enum AdapterType
        {
            LocalFileStorageAdapter,
            AzureBlobStorageAdapter,
            AmazonEC2StorageAdapter,
            GoogleBlobStorageAdapter
        }
    }
}
