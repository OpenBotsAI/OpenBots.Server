using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.Security;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class AspNetUsersRepository : TenantEntityRepository<AspNetUsers>, IAspNetUsersRepository
    {
        public AspNetUsersRepository(StorageContext context,  ILogger<AspNetUsers> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<AspNetUsers> DbTable()
        {
            return DbContext.AspNetUsers;
        }
    }
}
