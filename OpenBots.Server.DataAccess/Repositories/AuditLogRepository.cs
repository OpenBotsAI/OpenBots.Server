using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;

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
    }
}
