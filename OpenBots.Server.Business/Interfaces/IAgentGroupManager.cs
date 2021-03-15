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
        IEnumerable<AgentGroupMember> UpdateGroupMembers(string agentGroupId, IEnumerable<AgentGroupMember> groupMembers);
        AgentGroup UpdateAgentGroup(string id, AgentGroup request);
        void AttemptPatchUpdate(JsonPatchDocument<AgentGroup> request, Guid entityId);
        IEnumerable<AgentGroupMember> GetAllMembersInGroup(string agentGroupId);
        void DeleteGroupMembers(string agentGroupId);
    }
}
