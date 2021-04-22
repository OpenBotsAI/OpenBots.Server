using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.ViewModel;
using OpenBots.Server.Web.Webhooks;
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
        private readonly IWebhookPublisher _webhookPublisher;

        public AgentGroupManager(
            IAgentGroupRepository agentGroupRepository, 
            IAgentGroupMemberRepository agentGroupMemberRepository,
            IHttpContextAccessor httpContextAccessor,
            IAgentRepository agentRepository,
            IWebhookPublisher webhookPublisher
            )
        {
            _agentGroupRepository = agentGroupRepository;
            _agentGroupMemberRepository = agentGroupMemberRepository;
            _caller = ((httpContextAccessor.HttpContext != null) ? httpContextAccessor.HttpContext.User : new ClaimsPrincipal());
            _agentRepository = agentRepository;
            _webhookPublisher = webhookPublisher;
        }

        public override void SetContext(UserSecurityContext userSecurityContext)
        {
            _agentGroupRepository.SetContext(userSecurityContext);
            _agentGroupMemberRepository.SetContext(userSecurityContext);
            _agentRepository.SetContext(userSecurityContext);
            SecurityContext = userSecurityContext;
        }

        /// <summary>
        /// Updates the AgentGroupMembers of the specified AgentGroup id
        /// </summary>
        /// <param name="agentGroupId"></param>
        /// <param name="agentId"></param>
        /// <returns></returns>
        public IEnumerable<AgentGroupMember> UpdateGroupMembers(string agentGroupId, IEnumerable<AgentGroupMember> groupMembers)
        {
            var agentGroupGuid = Guid.Parse(agentGroupId);

            AgentGroup agentGroup = _agentGroupRepository.GetOne(agentGroupGuid);

            if (agentGroup == null)
            {
                throw new EntityDoesNotExistException("No agent group was found with the specified id");
            }

            List<AgentGroupMember> memberList = new List<AgentGroupMember>();

            DeleteGroupMembers(agentGroupId);//delete existing members

            foreach (var member in groupMembers ?? Enumerable.Empty<AgentGroupMember>())
            {
                member.AgentGroupId = agentGroupGuid;
                member.CreatedBy = _caller.Identity.Name;
                member.CreatedOn = DateTime.UtcNow;

                _agentGroupMemberRepository.Add(member);
                memberList.Add(member);
            }

            _webhookPublisher.PublishAsync("AgentGroups.AgentGroupMemberUpdated", agentGroupId, agentGroup.Name).ConfigureAwait(false);
            return memberList.AsEnumerable();
        }

        /// <summary>
        /// Takes an AgentGroup and returns it for addition
        /// </summary>
        /// <param name="agentGroup"></param>
        /// <returns>The AgentGroup to be added</returns>
        public AgentGroup AddAgentGroup(AgentGroup agentGroup)
        {
            var namedAgentGroup = _agentGroupRepository.Find(null, d => d.Name.ToLower() == agentGroup.Name.ToLower())?.Items?.FirstOrDefault();
            if (namedAgentGroup != null)
            {
                throw new EntityAlreadyExistsException("Agent group name already exists");
            }
            return agentGroup;
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

            var namedAgentGroup = _agentGroupRepository.Find(null, d => d.Name.ToLower() == request.Name.ToLower() && d.Id != entityId)?.Items?.FirstOrDefault();
            if (namedAgentGroup != null && namedAgentGroup.Id != entityId)
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
        public IEnumerable<AgentGroupMember> GetAllMembersInGroup(string agentGroupId)
        {
            var entityId = Guid.Parse(agentGroupId);
            var groupMemberList =_agentGroupMemberRepository.Find(null, a => a.AgentGroupId == entityId);

            return groupMemberList.Items.AsEnumerable();
        }

        /// <summary>
        /// Gets a list of all GroupMembers view in the specified AgentGroup
        /// </summary>
        /// <param name="agentGroupId"></param>
        /// <returns></returns>
        public IEnumerable<AgentGroupMemberViewModel> GetMembersView(string agentGroupId)
        {
            var groupMemberList =_agentGroupMemberRepository.GetMemberByGroupId(agentGroupId);

            return groupMemberList;
        }

        public void DeleteGroupMembers(string agentGroupId)
        {
            //delete all group members with this agent group id
            var allAgentGroupMembers = GetAllMembersInGroup(agentGroupId);
            foreach (var member in allAgentGroupMembers ?? Enumerable.Empty<AgentGroupMember>())
            {
                _agentGroupMemberRepository.Delete(member.Id ?? Guid.Empty);
            }
        }
    }
}
