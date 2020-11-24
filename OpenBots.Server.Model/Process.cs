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
        /// Id linked to Binary Object data table
        /// </summary>
        public Guid BinaryObjectId { get; set; }
        /// <summary>
        /// Original name of file
        /// </summary>
        public string OriginalPackageName { get; set; }
    }
}
