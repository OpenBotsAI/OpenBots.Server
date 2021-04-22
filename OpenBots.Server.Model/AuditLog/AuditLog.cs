using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model
{
    /// <summary>
    /// Audit Log model (inherits Entity model)
    /// </summary>
    public class AuditLog : Entity, INonAuditable
    {
        /// <summary>
        /// Id of object being changed
        /// </summary>
        public Guid? ObjectId { get; set; }
        /// <summary>
        /// Name of Service used
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// Name of Methos used
        /// </summary>
        public string MethodName { get; set; }
        /// <summary>
        /// Parameters needed to make the change
        /// </summary>
        public string ParametersJson { get; set; }
        /// <summary>
        /// Any exceptions that occurred while making changes to the entity
        /// </summary>
        public string ExceptionJson { get; set; }
        /// <summary>
        /// Information about entity before it was changed
        /// </summary>
        public string ChangedFromJson { get; set; }
        /// <summary>
        /// Information about entity after it was changed
        /// </summary>
        public string ChangedToJson { get; set; }
    }
}
