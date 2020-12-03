using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model.Configuration;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class EmailRepository : EntityRepository<EmailModel>, IEmailRepository
    {
        public EmailRepository (StorageContext storageContext, ILogger<EmailModel> logger, IHttpContextAccessor httpContextAccessor) : base(storageContext, logger, httpContextAccessor)
        { }

        protected override DbSet<EmailModel> DbTable()
        {
            return dbContext.Emails;
        }
    }
}
