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
        public virtual bool? IsChild { get; set; }
        public virtual string? ContentType { get; set; }
        public virtual string? CreatedBy { get; set; }
        public virtual DateTime? CreatedOn { get; set; }
        public virtual bool? IsFile { get; set; }
        public virtual Guid? ParentId { get; set; }
        public virtual string? FullStoragePath { get; set; }
        public virtual FileStream? Content { get; set; }
        public virtual IFormFile? File { get; set; }

        public FileFolderViewModel Map(ServerFile entity, string path)
        {
            FileFolderViewModel fileFolderView = new FileFolderViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                ContentType = entity.ContentType,
                CreatedBy = entity.CreatedBy,
                CreatedOn = entity.CreatedOn,
                FullStoragePath = entity.StoragePath,
                IsChild = true,
                IsFile = true,
                ParentId = entity.StorageFolderId,
                StoragePath = path,
                Size = entity.SizeInBytes
            };

            return fileFolderView;
        }

        public FileFolderViewModel Map(ServerFolder entity, string path)
        {
            FileFolderViewModel fileFolderView = new FileFolderViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                ContentType = "Folder",
                CreatedBy = entity.CreatedBy,
                CreatedOn = entity.CreatedOn,
                FullStoragePath = entity.StoragePath,
                IsChild = true,
                IsFile = false,
                ParentId = entity.ParentFolderId,
                StoragePath = path,
                Size = entity.SizeInBytes,
            };

            return fileFolderView;
        }
    }
}
