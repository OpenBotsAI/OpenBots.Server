using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model.Configuration;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class EmailRepository : EntityRepository<Email>, IEmailRepository
    {
        public EmailRepository (StorageContext storageContext, ILogger<Email> logger, IHttpContextAccessor httpContextAccessor) : base(storageContext, logger, httpContextAccessor)
        { }

        protected override DbSet<Email> DbTable()
        {
            return dbContext.Emails;
        }
    }
}
