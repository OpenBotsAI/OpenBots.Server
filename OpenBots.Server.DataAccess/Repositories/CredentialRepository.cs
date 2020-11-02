using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class CredentialRepository : EntityRepository<Credential>, ICredentialRepository
    {
        public CredentialRepository(StorageContext context, ILogger<Credential> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<Credential> DbTable()
        {
            return dbContext.Credentials;
        }
    }
}
