using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Webhooks;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class IntegrationEventLogRepository : EntityRepository<IntegrationEventLog>, IIntegrationEventLogRepository
    {
        public IntegrationEventLogRepository(StorageContext context, ILogger<IntegrationEventLog> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<IntegrationEventLog> DbTable()
        {
            return dbContext.IntegrationEventLogs;
        }
    }
}
