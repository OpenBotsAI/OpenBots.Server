using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.DataAccess.Repositories
{
    /// <summary>
    /// Audit Log Repository
    /// </summary>
    public class AuditLogRepository: EntityRepository<AuditLog>, IAuditLogRepository
    {
        /// <summary>
        /// Constructor for AuditLogRepository
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        public AuditLogRepository(StorageContext context, ILogger<AuditLog> logger, IHttpContextAccessor httpContextAccessor) :base(context, logger, httpContextAccessor)
        {
        }

        /// <summary>
        /// Retrieves audit logs
        /// </summary>
        /// <returns></returns>
        protected override DbSet<AuditLog> DbTable()
        {
            return dbContext.AuditLogs;
        }

        public string GetServiceName(AuditLog log)
        {
            var nameArray = log.ServiceName.Split(".");
            string name = string.Empty;
            for (var i = 3; i < nameArray.Length; i++)
            {
                if (string.IsNullOrEmpty(name))
                    name = nameArray[i];
                else
                    name += "." + nameArray[i];
            }

            if (log.ServiceName.Contains("BinaryObject"))
                name = "Files";
            else if (log.MethodName == "Login")
                name = "Identity.Auth";

            return name;
        }

        public PaginatedList<AuditLogViewModel> FindAllView(Predicate<AuditLogViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<AuditLogViewModel> paginatedList = new PaginatedList<AuditLogViewModel>();

            var auditLogsList = base.Find(null, j => j.IsDeleted == false);
            if (auditLogsList != null && auditLogsList.Items != null && auditLogsList.Items.Count > 0)
            {
                var auditLogRecord = from a in auditLogsList.Items
                                     select new AuditLogViewModel
                                     {
                                         Id = a?.Id,
                                         CreatedOn = a?.CreatedOn,
                                         CreatedBy = a?.CreatedBy,
                                         MethodName = a?.MethodName,
                                         ObjectId = a?.ObjectId,
                                         ServiceName = ((bool)(a?.MethodName.Contains("Login")) ? "Identity.Auth" : ((bool)(a?.ServiceName.Contains("BinaryObject")) ? "Files" : GetServiceName(a))) 
                                     };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        auditLogRecord = auditLogRecord.OrderBy(s => s.GetType().GetProperty(sortColumn).GetValue(s)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        auditLogRecord = auditLogRecord.OrderByDescending(s => s.GetType().GetProperty(sortColumn).GetValue(s)).ToList();

                List<AuditLogViewModel> filterRecord = null;
                if (predicate != null)
                    filterRecord = auditLogRecord.ToList().FindAll(predicate);
                else
                    filterRecord = auditLogRecord.ToList();

                paginatedList.Items = filterRecord.Skip(skip).Take(take).ToList();

                paginatedList.Completed = auditLogsList.Completed;
                paginatedList.Impediments = auditLogsList.Impediments;
                paginatedList.PageNumber = auditLogsList.PageNumber;
                paginatedList.PageSize = auditLogsList.PageSize;
                paginatedList.ParentId = auditLogsList.ParentId;
                paginatedList.Started = auditLogsList.Started;
                paginatedList.TotalCount = filterRecord?.Count;
            }

            return paginatedList;
        }
    }
}
