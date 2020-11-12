using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
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
