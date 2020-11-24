using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class QueueItemAttachmentRepository : EntityRepository<QueueItemAttachment>, IQueueItemAttachmentRepository
    {
        public QueueItemAttachmentRepository(StorageContext context, ILogger<QueueItemAttachment> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<QueueItemAttachment> DbTable()
        {
            return dbContext.QueueItemAttachments;
        }
    }
}
