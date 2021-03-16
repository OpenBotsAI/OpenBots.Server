using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public IEnumerable<AgentGroupMemberViewModel> GetMemberByGroupId(string id)
        {
            AgentGroupMemberViewModel agentViewModel = null;
            Guid agentGroupId;
            Guid.TryParse(id, out agentGroupId);

            var agentMember = base.Find(null, a => a.AgentGroupId == agentGroupId && a.IsDeleted == false);
            if (agentMember != null)
            {
                var agentView = from m in agentMember.Items
                                join g in dbContext.AgentGroups on m.AgentGroupId equals g.Id into table1
                                from g in table1.DefaultIfEmpty()
                                join a in dbContext.Agents on m.AgentId equals a.Id into table2 
                                from a in table2.DefaultIfEmpty()
                                select new AgentGroupMemberViewModel
                                {
                                    Id = m.Id,
                                    AgentGroupId = m.AgentGroupId,
                                    AgentId = m.AgentId,
                                    AgentGroupName = g.Name,
                                    AgentName = a.Name
                                };

                return agentView;
            }

            return Enumerable.Empty<AgentGroupMemberViewModel>();
        }
    }
}