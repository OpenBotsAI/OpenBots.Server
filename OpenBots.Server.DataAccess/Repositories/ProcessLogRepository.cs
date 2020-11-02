using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class ProcessLogRepository : EntityRepository<ProcessLog>, IProcessLogRepository
    {
        public ProcessLogRepository(StorageContext context, ILogger<ProcessLog> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {          
        }

        protected override DbSet<ProcessLog> DbTable()
        {
            return dbContext.ProcessLogs;
        }
    }
}
