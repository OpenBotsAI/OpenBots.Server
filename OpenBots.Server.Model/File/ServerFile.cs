using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.Model.File
{
    public class ServerFile: NamedEntity
    {
        public Guid? StorageFolderId { get; set; }
        public string ContentType { get; set; }
        public Guid? CorrelationEntityId { get; set; }
        public string CorrelationEntity { get; set; }
        public string StoragePath { get; set; }
        public string StorageProvider { get; set; }
        public long SizeInBytes { get; set; }
        public string HashCode { get; set; }
        public Guid? OrganizationId { get; set; }
        public List<FileAttribute> FileAttributes { get; set; }
    }
}