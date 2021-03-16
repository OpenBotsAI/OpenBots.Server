using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface IAgentGroupMemberRepository : IEntityRepository<AgentGroupMember>
    {
        IEnumerable<AgentGroupMemberViewModel> GetMemberByGroupId(string id);
    }
}
