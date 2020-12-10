using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model
{
    /// <summary>
    /// Automation model (inherits NamedEntity model)
    /// </summary>
    public class Automation : NamedEntity
    {
        /// <summary>
        /// Id linked to Binary Object data table
        /// </summary>
        public Guid BinaryObjectId { get; set; }
        /// <summary>
        /// Original name of file
        /// </summary>
        public string OriginalPackageName { get; set; }
        /// <summary>
        /// Type of automation that will be executed (i.e. OpenBots, Python, etc.)
        /// </summary>
        public string AutomationEngine { get; set; }
    }
}
