using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Webhooks;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class IntegrationEventSubscriptionAttemptRepository : EntityRepository<IntegrationEventSubscriptionAttempt>, IIntegrationEventSubscriptionAttemptRepository
    {
        public IntegrationEventSubscriptionAttemptRepository(StorageContext context, ILogger<IntegrationEventSubscriptionAttempt> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
    {
    }

    protected override DbSet<IntegrationEventSubscriptionAttempt> DbTable()
    {
        return dbContext.IntegrationEventSubscriptionAttempts;
    }
}
}
