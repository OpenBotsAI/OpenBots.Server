using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class AgentGroupRepository : EntityRepository<AgentGroup>, IAgentGroupRepository
    {
        public AgentGroupRepository(StorageContext context, ILogger<AgentGroup> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }
        protected override DbSet<AgentGroup> DbTable()
        {
            return dbContext.AgentGroups;
        }
    }
}
