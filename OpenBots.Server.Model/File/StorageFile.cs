using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.Model.File
{
    public class StorageFile: NamedEntity
    {
        public Guid? StorageFolderId { get; set; }
        public string ContentType { get; set; }
        public string StoragePath { get; set; }
        public string StorageProvider { get; set; }
        public long? SizeInBytes { get; set; }
        public string HashCode { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? StorageDriveId { get; set; }
        public List<FileAttribute> FileAttributes { get; set; }
    }
}