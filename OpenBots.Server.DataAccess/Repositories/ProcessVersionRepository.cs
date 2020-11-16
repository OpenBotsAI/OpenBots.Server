using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class ProcessVersionRepository : EntityRepository<ProcessVersion>, IProcessVersionRepository
    {
        public ProcessVersionRepository(StorageContext context, ILogger<ProcessVersion> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<ProcessVersion> DbTable()
        {
            return dbContext.ProcessVersions;
        }
    }
}
