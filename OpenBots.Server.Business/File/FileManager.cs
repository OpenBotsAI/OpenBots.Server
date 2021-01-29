using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Business.File
{
    public class FileManager : BaseManager, IFileManager
    {
        private readonly LocalFileStorageAdapter localFileStorageAdapter;

        public IConfiguration Configuration { get; }

        public FileManager(
            IConfiguration configuration,
            LocalFileStorageAdapter localFileStorageAdapter)
        {
            Configuration = configuration;
            this.localFileStorageAdapter = localFileStorageAdapter;
        }

        public PaginatedList<FileFolderViewModel> GetFilesFolders(bool? isFile, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            string adapter = Configuration["Files:Adapter"];

            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
                return localFileStorageAdapter.GetFilesFolders(isFile, predicate, sortColumn, direction, skip, take);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

        }

        public int? GetFolderCount()
        {
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
            {
                int? count = localFileStorageAdapter.GetFolderCount();
                return count;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public FileFolderViewModel GetFileFolder(string id)
        {
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
            {
                var folder = localFileStorageAdapter.GetFileFolderViewModel(id);
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

        public ServerDrive GetDrive(string path)
        {
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
            {
                var drive = localFileStorageAdapter.GetDriveByName(path);
                return drive;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public FileFolderViewModel AddFileFolder(FileFolderViewModel request)
        {
            FileFolderViewModel response = new FileFolderViewModel();
            string adapter = Configuration["Files:Adapter"];

            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
                response = localFileStorageAdapter.AddFileFolder(request);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
        }

        public async Task<FileFolderViewModel> ExportFileFolder(string id)
        {
            var response = new FileFolderViewModel();
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
                response = await localFileStorageAdapter.ExportFile(id);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityOperationException("Configuration is not set up for local file storage");

            return response;
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
