using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Webhooks;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class IntegrationEventRepository : EntityRepository<IntegrationEvent>, IIntegrationEventRepository
    {
        public IntegrationEventRepository(StorageContext context, ILogger<IntegrationEvent> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<IntegrationEvent> DbTable()
        {
            return dbContext.IntegrationEvents;
        }
    }
}
