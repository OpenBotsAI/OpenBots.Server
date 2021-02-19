using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IAgentGroupManager : IManager
    {
        AgentGroupMember CreateNewGroupMember(string agentGroupId, string agentId);
        AgentGroup UpdateAgentGroup(string id, AgentGroup request);
        void AttemptPatchUpdate(JsonPatchDocument<AgentGroup> request, string id);
        PaginatedList<AgentGroupMember> GetAllMembersInGroup(string agentGroupId);
        void DeleteGroupMembers(string agentGroupId);
    }
}
