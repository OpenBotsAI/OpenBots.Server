using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Webhooks;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class IntegrationEventSubscriptionRepository : EntityRepository<IntegrationEventSubscription>, IIntegrationEventSubscriptionRepository
    {
        public IntegrationEventSubscriptionRepository(StorageContext context, ILogger<IntegrationEventSubscription> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<IntegrationEventSubscription> DbTable()
        {
            return dbContext.IntegrationEventSubscriptions;
        }
    }
}
