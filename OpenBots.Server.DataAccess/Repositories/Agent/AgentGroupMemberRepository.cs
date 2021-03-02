using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;


namespace OpenBots.Server.DataAccess.Repositories
{
    public class AgentGroupMemberRepository : EntityRepository<AgentGroupMember>, IAgentGroupMemberRepository
    {
        public AgentGroupMemberRepository(StorageContext context, ILogger<AgentGroupMember> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }
        protected override DbSet<AgentGroupMember> DbTable()
        {
            return dbContext.AgentGroupMembers;
        }
    }
}