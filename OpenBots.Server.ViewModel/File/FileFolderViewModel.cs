using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.File;
using System;
using System.IO;

namespace OpenBots.Server.ViewModel.File
{
    public class FileFolderViewModel
    {
        public Guid? Id { get; set; }
        public virtual string? Name { get; set; }
        public virtual long? Size { get; set; }
        public virtual string? StoragePath { get; set; }
        public virtual string? FullStoragePath { get; set; }
        public virtual bool? HasChild { get; set; }
        public virtual string? ContentType { get; set; }
        public virtual string? CreatedBy { get; set; }
        public virtual DateTime? CreatedOn { get; set; }
        public virtual DateTime? UpdatedOn { get; set; }
        public virtual bool? IsFile { get; set; }
        public virtual Guid? ParentId { get; set; }
        public virtual Guid? StorageDriveId { get; set; }
        public virtual FileStream? Content { get; set; }
        public virtual IFormFile[]? Files { get; set; }
        public virtual string Hash { get; set; }

        public FileFolderViewModel Map(StorageFile entity, string path)
        {
            FileFolderViewModel fileFolderView = new FileFolderViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                ContentType = entity.ContentType,
                CreatedBy = entity.CreatedBy,
                CreatedOn = entity.CreatedOn,
                UpdatedOn = entity.UpdatedOn,
                FullStoragePath = entity.StoragePath,
                HasChild = false,
                IsFile = true,
                ParentId = entity.StorageFolderId,
                StoragePath = path,
                Size = entity.SizeInBytes,
                StorageDriveId = entity.StorageDriveId,
                Hash = entity.HashCode
            };

            return fileFolderView;
        }

        public FileFolderViewModel Map(StorageFolder entity, string path, bool hasChild)
        {
            FileFolderViewModel fileFolderView = new FileFolderViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                ContentType = "Folder",
                CreatedBy = entity.CreatedBy,
                CreatedOn = entity.CreatedOn,
                UpdatedOn = entity.UpdatedOn,
                FullStoragePath = entity.StoragePath,
                HasChild = hasChild,
                IsFile = false,
                ParentId = entity.ParentFolderId,
                StoragePath = path,
                Size = entity.SizeInBytes,
                StorageDriveId = entity.StorageDriveId
            };

            return fileFolderView;
        }
    }
}
