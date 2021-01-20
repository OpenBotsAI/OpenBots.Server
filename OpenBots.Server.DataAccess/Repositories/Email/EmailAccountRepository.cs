using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Core;
using OpenBots.Server.Model.Configuration;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class EmailAccountRepository : EntityRepository<EmailAccount>, IEmailAccountRepository
    {
        public EmailAccountRepository(StorageContext storageContext, ILogger<EmailAccount> logger, IHttpContextAccessor httpContextAccessor) : base(storageContext, logger, httpContextAccessor)
        {
        }

        protected override DbSet<EmailAccount> DbTable()
        {
            return DbContext.EmailAccounts;
        }
    }
}
