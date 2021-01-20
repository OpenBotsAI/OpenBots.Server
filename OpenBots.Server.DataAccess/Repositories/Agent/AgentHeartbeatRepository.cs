using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class AgentHeartbeatRepository : EntityRepository<AgentHeartbeat>, IAgentHeartbeatRepository
    {
        public AgentHeartbeatRepository(StorageContext context, ILogger<AgentHeartbeat> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<AgentHeartbeat> DbTable()
        {
            return dbContext.AgentHeartbeats;
        }
    }
}
