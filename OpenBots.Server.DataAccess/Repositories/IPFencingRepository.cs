using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class IPFencingRepository : EntityRepository<IPFencing>, IIPFencingRepository
    {
        public IPFencingRepository(StorageContext context, ILogger<IPFencing> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<IPFencing> DbTable()
        {
            return dbContext.IPFencings;
        }
    }
}
