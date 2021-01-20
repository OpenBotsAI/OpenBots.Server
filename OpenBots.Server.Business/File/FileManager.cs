using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.ViewModel.File;
using Syncfusion.EJ2.FileManager.Base;
using System.Collections.Generic;

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

        public object LocalFileStorageOperation(FileManagerDirectoryContent args)
        {
            var file = new FileViewModel();
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
                return localFileStorageAdapter.LocalFileStorageOperation(args);
            else throw new EntityOperationException("Configuration is not set up for local file storage");
        }

        public FileManagerResponse UploadFile(string path, IList<IFormFile> uploadFiles, string action)
        {
            string storageProvider = Configuration["Files:StorageProvider"];
            string adapter = Configuration["Files:Adapter"];
            var content = new FileManagerResponse();
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()) && storageProvider.Equals("FileSystem.Default"))
                content = localFileStorageAdapter.UploadFile(path, uploadFiles, action);
            //else if (adapter.Equals("AzureBlobStorageAdapter") && storageProvider.Equals("FileSystem.Azure"))
            //    azureBlobStorageAdapter.SaveFile(request);
            //else if (adapter.Equals("AmazonEC2StorageAdapter") && storageProvider.Equals("FileSystem.Amazon"))
            //    amazonEC2StorageAdapter.SaveFile(request);
            //else if (adapter.Equals("GoogleBlobStorageAdapter") && storageProvider.Equals("FileSystem.Google"))
            //    googleBlobStorageAdapter.SaveFile(request);
            else throw new EntityDoesNotExistException("Configuration for file storage is not configured or cannot not be found");

            return content;
        }

        public object DownloadFile(string downloadInput)
        {
            var content = new object();
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
                content = localFileStorageAdapter.DownloadFile(downloadInput);
            //else if (adapter.Equals(AdapterType.AzureBlobStorageAdapter.ToString()))
            //    content = azureBlobStorageAdapter.DownloadFile(downloadInput);
            //else if (adapter.Equals(AdapterType.AmazonEC2StorageAdapter.ToString()))
            //    content = amazonEC2StorageAdapter.DownloadFile(downloadInput);
            //else if (adapter.Equals(AdapterType.GoogleBlobStorageAdapter.ToString()))
            //    content = googleBlobStorageAdapter.DownloadFile(downloadInput);
            else throw new EntityDoesNotExistException("Configuration for file storage is not configured or cannot not be found");

            return content;
        }

        public object GetImage(FileManagerDirectoryContent args)
        {
            string adapter = Configuration["Files:Adapter"];
            if (adapter.Equals(AdapterType.LocalFileStorageAdapter.ToString()))
                return localFileStorageAdapter.GetImage(args);
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
