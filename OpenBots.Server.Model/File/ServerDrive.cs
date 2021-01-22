using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model.File
{
    public class ServerDrive : NamedEntity
    {
        public string FileStorageAdapterType { get; set; }
        public Guid? OrganizationId { get; set; }
        public long? StorageSizeInBytes { get; set; }
    }
}
