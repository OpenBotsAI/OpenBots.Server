using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class AutomationLogRepository : EntityRepository<AutomationLog>, IAutomationLogRepository
    {
        public AutomationLogRepository(StorageContext context, ILogger<AutomationLog> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {          
        }

        protected override DbSet<AutomationLog> DbTable()
        {
            return dbContext.AutomationLogs;
        }
    }
}
