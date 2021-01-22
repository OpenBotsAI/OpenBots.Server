using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface IAgentHeartbeatRepository : IEntityRepository<AgentHeartbeat>
    {
        PaginatedList<AgentHeartbeat> FindAllHeartbeats(Guid agentId, Predicate<AgentHeartbeat> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
    }
}

