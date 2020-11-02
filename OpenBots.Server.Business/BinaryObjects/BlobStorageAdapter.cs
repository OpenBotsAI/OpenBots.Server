using Microsoft.AspNetCore.Http;
using OpenBots.Server.DataAccess.Repositories;
using System;
using System.IO;
using OpenBots.Server.ViewModel;
using System.Threading.Tasks;
using OpenBots.Server.Model;

namespace OpenBots.Server.Business
{
    public class BlobStorageAdapter : IBlobStorageAdapter
    {
        private readonly IBinaryObjectRepository repo;
        public BlobStorageAdapter(IBinaryObjectRepository repo)
        {
            this.repo = repo;
        }

        public async void SaveEntity(IFormFile file, string filePath, BinaryObject binaryObject, string apiComponent, string organizationId, string storageProvider, string hash)
        {
            binaryObject.ContentType = file.ContentType;
            binaryObject.CorrelationEntity = apiComponent;
            binaryObject.HashCode = hash;
            binaryObject.OrganizationId = Guid.Parse(organizationId);
            binaryObject.SizeInBytes = file.Length;
            binaryObject.StoragePath = filePath;
            binaryObject.StorageProvider = storageProvider;

            repo.Update(binaryObject);
        }

        public async Task<FileObjectViewModel> FetchFile(string binaryObjectId)
        {
            FileObjectViewModel file = new FileObjectViewModel();

            var binaryObject = repo.GetOne(Guid.Parse(binaryObjectId));

            if (binaryObject != null)
            {
                file.StoragePath = binaryObject.StoragePath;
                file.Name = binaryObject.Name;
                file.ContentType = binaryObject.ContentType;
                file.BlobStream = new FileStream(file?.StoragePath, FileMode.Open, FileAccess.Read);
            }

            return file;
        }

        public async Task<string> UpdateEntity(IFormFile file, string filePath, string binaryObjectId, string apiComponent, string folder, string name, string hash)
        {
            //update binary object entity properties in data table
            await repo.UpdateEntity(file, filePath, binaryObjectId, apiComponent, folder, name, hash);
            return "Success";
        }
    }
}
