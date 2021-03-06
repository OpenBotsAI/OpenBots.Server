﻿using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model.File
{
    public class StorageDrive : NamedEntity
    {
        public string? FileStorageAdapterType { get; set; }
        public Guid? OrganizationId { get; set; }
        public string? StoragePath { get; set; }
        public long? StorageSizeInBytes { get; set; }
        public long? MaxStorageAllowedInBytes { get; set; }
        public bool? IsDefault { get; set; }
    }
}