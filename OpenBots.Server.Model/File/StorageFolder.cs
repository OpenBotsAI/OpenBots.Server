using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model.File
{
    public class StorageFolder: NamedEntity
    {
        public Guid? StorageDriveId { get; set; }
        public Guid? ParentFolderId { get; set; }
        public Guid? OrganizationId { get; set; }
        public string StoragePath { get; set; }
        public long? SizeInBytes { get; set; }
    }
}
