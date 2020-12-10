using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class AutomationVersionRepository : EntityRepository<AutomationVersion>, IAutomationVersionRepository
    {
        public AutomationVersionRepository(StorageContext context, ILogger<AutomationVersion> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<AutomationVersion> DbTable()
        {
            return dbContext.AutomationVersions;
        }
    }
}
