using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;

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

        public FileViewModel GetFile(string path)
        {
            var file = new FileViewModel();
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
                file = localFileStorageAdapter.GetFile(path);
            //else if (adapter.Equals(AdapterType.AzureBlobStorageAdapter.ToString()))
            //    file = azureBlobStorageAdapter.GetFile(fileId);
            //else if (adapter.Equals(AdapterType.AmazonEC2StorageAdapter.ToString()))
            //    file = amazonEC2StorageAdapter.GetFile(fileId);
            //else if (adapter.Equals(AdapterType.GoogleBlobStorageAdapter.ToString()))
            //    file = googleBlobStorageAdapter.GetFile(fileId);
            else throw new EntityDoesNotExistException("Configuration for file storage is not configured or cannot not be found");

            return file;
        }

        public FileViewModel SaveFile(SaveServerFileViewModel request)
        {
            string storageProvider = Configuration["Files:StorageProvider"];
            string adapter = Configuration["Files:Adapter"];
            var file = new FileViewModel();
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()) && storageProvider.Equals("FileSystem.Default"))
                file = localFileStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityDoesNotExistException("Configuration for file storage is not configured or cannot not be found");

            return file;
        }

        public FileViewModel UpdateFile(UpdateServerFileViewModel request)
        {
            var file = new FileViewModel();
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
                file = localFileStorageAdapter.UpdateFile(request);
            //else if (adapter.Equals(AdapterType.AzureBlobStorageAdapter.ToString()))
            //    file = azureBlobStorageAdapter.GetFile(fileId);
            //else if (adapter.Equals(AdapterType.AmazonEC2StorageAdapter.ToString()))
            //    file = amazonEC2StorageAdapter.GetFile(fileId);
            //else if (adapter.Equals(AdapterType.GoogleBlobStorageAdapter.ToString()))
            //    file = googleBlobStorageAdapter.GetFile(fileId);
            else throw new EntityDoesNotExistException("Configuration for file storage is not configured or cannot be found");

            return file;
        }

        public void DeleteFile(string path)
        {
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
                localFileStorageAdapter.DeleteFile(path);
            //else if (adapter.Equals(AdapterType.AzureBlobStorageAdapter.ToString()))
            //    file = azureBlobStorageAdapter.DeleteFile(path);
            //else if (adapter.Equals(AdapterType.AmazonEC2StorageAdapter.ToString()))
            //    file = amazonEC2StorageAdapter.DeleteFile(path);
            //else if (adapter.Equals(AdapterType.GoogleBlobStorageAdapter.ToString()))
            //    file = googleBlobStorageAdapter.DeleteFile(path);
            else throw new EntityDoesNotExistException("Configuration for file storage is not configured or cannot be found");
        }

        public int? GetFolderCount()
        {
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
            {
                int? count = localFileStorageAdapter.GetFolderCount();
                return count;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage.");
        }

        public ServerFolder GetFolder(string name)
        {
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
            {
                var folder = localFileStorageAdapter.GetFolder(name);
                return folder;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage.");
        }

        public ServerDrive GetDrive()
        {
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
            {
                var drive = localFileStorageAdapter.GetDrive();
                return drive;
            }
            else throw new EntityOperationException("Configuration is not set up for local file storage.");
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
