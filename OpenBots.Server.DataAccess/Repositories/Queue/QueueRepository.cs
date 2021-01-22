using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class QueueRepository : EntityRepository<Queue>, IQueueRepository
    {
        public QueueRepository(StorageContext context, ILogger<Queue> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<Queue> DbTable()
        {
            return dbContext.Queues;
        }
    }
}
