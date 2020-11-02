using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model
{
    /// <summary>
    /// Process model (inherits NamedEntity model)
    /// </summary>
    public class Process : NamedEntity
    {
        /// <summary>
        /// Version of Process
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// Status of Process
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Id linked to Binary Object data table
        /// </summary>
        public Guid BinaryObjectId { get; set; }
        /// <summary>
        /// Id to match other versions of the same process
        /// </summary>
        public Guid VersionId { get; set; }
    }
}
