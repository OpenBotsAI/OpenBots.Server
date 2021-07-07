using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class AgentSettingRepository : EntityRepository<AgentSetting>, IAgentSettingRepository
    {
        public AgentSettingRepository(StorageContext context, ILogger<AgentSetting> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }
        protected override DbSet<AgentSetting> DbTable()
        {
            return dbContext.AgentSettings;
        }
    }
}
