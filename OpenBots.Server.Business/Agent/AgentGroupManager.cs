using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace OpenBots.Server.Business
{
    public class AgentGroupManager : BaseManager, IAgentGroupManager
    {
        private readonly IAgentGroupRepository _agentGroupRepository;
        private readonly IAgentGroupMemberRepository _agentGroupMemberRepository;
        private readonly ClaimsPrincipal _caller;
        private readonly IAgentRepository _agentRepository;

        public AgentGroupManager(
            IAgentGroupRepository agentGroupRepository, 
            IAgentGroupMemberRepository agentGroupMemberRepository,
            IHttpContextAccessor httpContextAccessor,
            IAgentRepository agentRepository
            )
        {
            _agentGroupRepository = agentGroupRepository;
            _agentGroupMemberRepository = agentGroupMemberRepository;
            _caller = ((httpContextAccessor.HttpContext != null) ? httpContextAccessor.HttpContext.User : new ClaimsPrincipal());
            _agentRepository = agentRepository;
        }

        public override void SetContext(UserSecurityContext userSecurityContext)
        {
            _agentGroupRepository.SetContext(userSecurityContext);
            _agentGroupMemberRepository.SetContext(userSecurityContext);
            SecurityContext = userSecurityContext;
        }

        /// <summary>
        /// Creates a new AgentGroupMember entity with the specified ids
        /// </summary>
        /// <param name="agentGroupId"></param>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public AgentGroupMember CreateNewGroupMember(string agentGroupId, string agentId)
        {
            var agentGroupGuid = Guid.Parse(agentGroupId);
            var agentGuid = Guid.Parse(agentId);

            AgentGroup agentGroup = _agentGroupRepository.GetOne(agentGroupGuid);
            Agent agent = _agentRepository.GetOne(agentGuid);

            if (agentGroup == null)
            {
                throw new EntityDoesNotExistException("No agent group was found with the specified id");
            }
            if (agent == null)
            {
                throw new EntityDoesNotExistException("No agent was found with the specified id");
            }

            AgentGroupMember agentGroupMember = new AgentGroupMember() 
            {
                AgentGroupId = Guid.Parse(agentGroupId),
                AgentId = Guid.Parse(agentId),
                CreatedBy = _caller.Identity.Name,
                CreatedOn = DateTime.UtcNow
            };
            return _agentGroupMemberRepository.Add(agentGroupMember);
        }

        /// <summary>
        /// Updates an AgentGroup entity 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public AgentGroup UpdateAgentGroup(string id, AgentGroup request)
        {
            Guid entityId = new Guid(id);

            var existingAgentGroup = _agentGroupRepository.GetOne(entityId);
            if (existingAgentGroup == null)
            {
                throw new EntityDoesNotExistException("No agent group exists for the specified agent group id");
            }

            var namedAgent = _agentGroupRepository.Find(null, d => d.Name.ToLower() == request.Name.ToLower() && d.Id != entityId)?.Items?.FirstOrDefault();
            if (namedAgent != null && namedAgent.Id != entityId)
            {
                throw new EntityAlreadyExistsException("Agent group name already exists");
            }

            existingAgentGroup.Name = request.Name;
            existingAgentGroup.IsEnabled = request.IsEnabled;
            existingAgentGroup.Description = request.Description;

            return existingAgentGroup;
        }

        /// <summary>
        /// Verifies that the patch update can be completed
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        public void AttemptPatchUpdate(JsonPatchDocument<AgentGroup> request, Guid entityId)
        {
            for (int i = 0; i < request.Operations.Count; i++)
            {
                if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/name")
                {
                    var namedAgentGroup = _agentGroupRepository.Find(null, d => d.Name.ToLower() == request.Operations[i].value.ToString().ToLower() && d.Id != entityId)?.Items?.FirstOrDefault();
                    if (namedAgentGroup != null && namedAgentGroup.Id != entityId)
                    {
                        throw new EntityAlreadyExistsException("Agent group name already exists");
                    }
                }
            }
        }

        /// <summary>
        /// Gets a list of all GroupMembers in the specified AgentGroup
        /// </summary>
        /// <param name="agentGroupId"></param>
        /// <returns></returns>
        public PaginatedList<AgentGroupMember> GetAllMembersInGroup(string agentGroupId)
        {
            var entityId = Guid.Parse(agentGroupId);
            var groupMemberList =_agentGroupMemberRepository.Find(null, a => a.AgentGroupId == entityId);

            return groupMemberList;
        }

        public void DeleteGroupMembers(string agentGroupId)
        {
            //delete all group members with this agent group id
            var allAgentGroupMembers = GetAllMembersInGroup(agentGroupId).Items;
            foreach (var member in allAgentGroupMembers)
            {
                _agentGroupMemberRepository.SoftDelete(member.Id ?? Guid.Empty);
            }
        }
    }
}
