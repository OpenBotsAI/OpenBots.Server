using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel
{
    public class BinaryObjectViewModel : IViewModel<BinaryObject, BinaryObjectViewModel>
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public Guid? OrganizationId { get; set; }
        public string ContentType { get; set; }
        public Guid? CorrelationEntityId { get; set; }
        public string CorrelationEntity { get; set; }
        public string StoragePath { get; set; }
        public string Folder { get; set; }
        public string StorageProvider { get; set; }
        public long? SizeInBytes { get; set; }
        public string HashCode { get; set; }
        public IFormFile? File { get; set; }

        public BinaryObjectViewModel Map(BinaryObject entity)
        {
            BinaryObjectViewModel binaryObjectViewModel = new BinaryObjectViewModel();

            binaryObjectViewModel.Id = entity.Id;
            binaryObjectViewModel.Name = entity.Name;
            binaryObjectViewModel.OrganizationId = entity.OrganizationId;
            binaryObjectViewModel.ContentType = entity.ContentType;
            binaryObjectViewModel.CorrelationEntityId = entity.CorrelationEntityId;
            binaryObjectViewModel.CorrelationEntity = entity.CorrelationEntity;
            binaryObjectViewModel.Folder = entity.Folder;
            binaryObjectViewModel.StoragePath = entity.StoragePath;
            binaryObjectViewModel.SizeInBytes = entity.SizeInBytes;
            binaryObjectViewModel.HashCode = entity.HashCode;

            return binaryObjectViewModel;
        }
    }
}
