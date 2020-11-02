using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class QueueItemRepository : EntityRepository<QueueItem>, IQueueItemRepository
    {
        public QueueItemRepository(StorageContext context, ILogger<QueueItem> logger,IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<QueueItem> DbTable()
        {
            return dbContext.QueueItems;
        }
    }
}
