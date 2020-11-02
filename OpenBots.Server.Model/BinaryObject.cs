using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model
{
    /// <summary>
    /// Binary Object data model
    /// </summary>
    public class BinaryObject : NamedEntity
    {
        /// <summary>
        /// Organization Id
        /// </summary>
        public virtual Guid? OrganizationId { get; set; }
        /// <summary>
        /// Content Type
        /// </summary>
        public virtual string ContentType { get; set; }
        /// <summary>
        /// Correlation Identity Id
        /// </summary>
        public virtual Guid? CorrelationEntityId { get; set; }
        /// <summary>
        /// Correlation Identity
        /// </summary>
        public virtual string CorrelationEntity { get; set; }
        /// <summary>
        /// Folder
        /// </summary>
        public virtual string Folder { get; set; }
        /// <summary>
        /// Storage Path
        /// </summary>
        public virtual string StoragePath { get; set; }
        /// <summary>
        /// Storage Provider
        /// </summary>
        public virtual string StorageProvider { get; set; }
        /// <summary>
        /// Size in Bytes
        /// </summary>
        public virtual long? SizeInBytes { get; set; }
        /// <summary>
        /// Hash Code
        /// </summary>
        public virtual string HashCode { get; set; }
    }
}
