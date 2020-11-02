using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel
{
    /// <summary>
    /// ViewModel for Audit Log model
    /// </summary>
    public class AuditLogViewModel : IViewModel<AuditLog, AuditLogViewModel>
    {
        /// <summary>
        /// Name of Service used
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// Name of Method used
        /// </summary>
        public string MethodName { get; set; }
        /// <summary>
        /// Who made the changes to the entity
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        ///When update was made
        /// </summary>
        public DateTime? CreatedOn { get; set; }
        /// <summary>
        /// Id of object being changed
        /// </summary>
        public Guid? ObjectId { get; set; }
        /// <summary>
        /// Id of audit log
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Mapping for entity and viewmodel
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public AuditLogViewModel Map(AuditLog entity)
        {
            AuditLogViewModel auditLogViewModel = new AuditLogViewModel();

            auditLogViewModel.CreatedBy = entity.CreatedBy;
            auditLogViewModel.CreatedOn = entity.CreatedOn;
            auditLogViewModel.ServiceName = entity.ServiceName;
            auditLogViewModel.MethodName = entity.MethodName;
            auditLogViewModel.ObjectId = entity.ObjectId;
            auditLogViewModel.Id = entity.Id;

            return auditLogViewModel;
        }
    }
}
