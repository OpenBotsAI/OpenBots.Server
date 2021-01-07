using OpenBots.Server.Model.Core;
using System;
using AuditLogModel = OpenBots.Server.Model.AuditLog;

namespace OpenBots.Server.ViewModel.AuditLog
{
    public class AuditLogDetailsViewModel : Entity, IViewModel<AuditLogModel, AuditLogDetailsViewModel>
    {
        public Guid? ObjectId { get; set; }
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public string ParametersJson { get; set; }
        public string ExceptionJson { get; set; }
        public string ChangedFromJson { get; set; }
        public string ChangedToJson { get; set; }

        public AuditLogDetailsViewModel Map(AuditLogModel entity)
        {
            AuditLogDetailsViewModel auditLogViewModel = new AuditLogDetailsViewModel()
            {
                ChangedFromJson = entity.ChangedFromJson,
                ChangedToJson = entity.ChangedToJson,
                CreatedBy = entity.CreatedBy,
                CreatedOn = entity.CreatedOn,
                DeletedBy = entity.DeletedBy,
                DeleteOn = entity.DeleteOn,
                Id = entity.Id,
                IsDeleted = entity.IsDeleted,
                ObjectId = entity.ObjectId,
                UpdatedOn = entity.UpdatedOn,
                UpdatedBy = entity.UpdatedBy,
                ExceptionJson = entity.ExceptionJson,
                MethodName = entity.MethodName,
                ParametersJson = entity.ParametersJson,
                ServiceName = entity.ServiceName,
                Timestamp = entity.Timestamp
            };

            return auditLogViewModel;
        }
    }
}
