using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model.File
{
    public class FileAttribute : NamedEntity
    {
        public Guid? StorageFileId { get; set; }
        public int AttributeValue { get; set; }
        public string DataType { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid? StorageDriveId { get; set; }
    }
}
