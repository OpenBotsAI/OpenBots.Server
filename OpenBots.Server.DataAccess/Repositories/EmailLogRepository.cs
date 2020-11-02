using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model.Configuration;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class EmailLogRepository : EntityRepository<EmailLog>, IEmailLogRepository
    {
        public EmailLogRepository (StorageContext storageContext, ILogger<EmailLog> logger, IHttpContextAccessor httpContextAccessor) : base(storageContext, logger, httpContextAccessor)
        { }

        protected override DbSet<EmailLog> DbTable()
        {
            return dbContext.EmailLogs;
        }
    }
}
