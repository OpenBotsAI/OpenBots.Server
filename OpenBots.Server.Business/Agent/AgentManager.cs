﻿using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.AgentViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.Business
{
    public class AgentManager : BaseManager, IAgentManager
    {
        private readonly IAgentRepository _agentRepo;
        private readonly IScheduleRepository _scheduleRepo;
        private readonly IJobRepository _jobRepo;
        private readonly IAspNetUsersRepository _usersRepo;
        private readonly ICredentialRepository _credentialRepo;
        private readonly IAgentHeartbeatRepository _agentHeartbeatRepo;
        private readonly ApplicationIdentityUserManager _userManager;
        private readonly IPersonRepository _personRepo;

        public AgentManager(IAgentRepository agentRepository,
            IScheduleRepository scheduleRepository,
            IJobRepository jobRepository,
            IAspNetUsersRepository usersRepository,
            ICredentialRepository credentialRepository,
            IAgentHeartbeatRepository agentHeartbeatRepository,
            ApplicationIdentityUserManager userManager,
            IPersonRepository personRepository)
        {
            _agentRepo = agentRepository;
            _scheduleRepo = scheduleRepository;
            _jobRepo = jobRepository;
            _usersRepo = usersRepository;
            _credentialRepo = credentialRepository;
            _agentHeartbeatRepo = agentHeartbeatRepository;
            _userManager = userManager;
            _personRepo = personRepository;
        }

        /// <summary>
        /// Creates necessary user tables for the provided request
        /// </summary>
        /// <param name="request"></param>
        public void CreateAgentUserAccount(CreateAgentViewModel request)
        {
            //name must be unique
            Agent namedAgent = _agentRepo.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null))?.Items?.FirstOrDefault();
            if (namedAgent != null)
            {
                throw new EntityAlreadyExistsException("Agent name already exists");
            }

            Guid entityId = Guid.NewGuid();
            if (request.Id == null || !request.Id.HasValue || request.Id.Equals(Guid.Empty))
                request.Id = entityId;

            //create IsAgent person
            Person newPerson = new Person()
            {
                Name = request.Name,
                IsAgent = true
            };

            if (newPerson == null)
            {
                throw new EntityOperationException("Failed to create agent user");
            }

            //create agent app user
            var user = new ApplicationUser()
            {
                Name = request.Name,
                UserName = request.UserName,
                PersonId = newPerson.Id ?? Guid.Empty
            };

            var loginResult = _userManager.CreateAsync(user, request.Password).Result;
            var errors = loginResult.Errors;

            if (errors.Any())
            {

                foreach (var error in errors)
                {
                    if (error.Code == "DuplicateUserName")
                    {
                        throw new EntityAlreadyExistsException("UserName already exists");
                    }
                }

                throw new EntityOperationException("Failed to create new agent credentials");
            }
            else
            {
                _personRepo.Add(newPerson);
            }
        }

        /// <summary>
        /// Deletes all dependencies for the specified agent
        /// </summary>
        /// <param name="agentId"></param>
        public void DeleteAgentDependencies(Agent agent)
        {
            bool childExists = CheckReferentialIntegrity(agent.Id.ToString());
            if (childExists)
            {
                throw new EntityOperationException("Referential Integrity in Schedule or job table, please remove those before deleting this agent");
            }

            _personRepo.ForceIgnoreSecurity();
            Person person = _personRepo.Find(0, 1).Items?.Where(p => p.IsAgent && p.Name == agent.Name && !(p.IsDeleted ?? false))?.FirstOrDefault();
            if (person != null)
            {
                _personRepo.SoftDelete((Guid)person.Id);
            }
            _personRepo.ForceSecurity();

            var aspUser = _usersRepo.Find(0, 1).Items?.Where(u => u.PersonId == person.Id)?.FirstOrDefault();

            if (aspUser != null)
            {
                var user = _userManager.FindByIdAsync(aspUser.Id).Result;
                var deleteResult = _userManager.DeleteAsync(user).Result;

                if (!deleteResult.Succeeded)
                {
                    throw new EntityOperationException("Something went wrong, unable to delete agent user");
                }
            }

            DeleteExistingHeartbeats(agent.Id ?? Guid.Empty);
        }

        /// <summary>
        /// Updates an Agent's user account and returns the updated Agent
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public Agent UpdateAgent(string id, Agent request)
        {
            Guid entityId = new Guid(id);

            var existingAgent = _agentRepo.GetOne(entityId);
            if (existingAgent == null)
            {
                throw new EntityDoesNotExistException("No agent exists for the specified agent id");
            }

            var namedAgent = _agentRepo.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null) && d.Id != entityId)?.Items?.FirstOrDefault();
            if (namedAgent != null && namedAgent.Id != entityId)
            {
                throw new EntityAlreadyExistsException("Agent Name Already Exists");
            }

            if (existingAgent.Name != request.Name)
            {
                UpdateAgentName(existingAgent.Name, request.Name);
            }

            existingAgent.Name = request.Name;
            existingAgent.MachineName = request.MachineName;
            existingAgent.MacAddresses = request.MacAddresses;
            existingAgent.IPAddresses = request.IPAddresses;
            existingAgent.IsEnabled = request.IsEnabled;
            existingAgent.CredentialId = request.CredentialId;
            existingAgent.IPOption = request.IPOption;
            existingAgent.IsEnhancedSecurity = request.IsEnhancedSecurity;

            return existingAgent;
        }

        /// <summary>
        /// Updates an Agent's name in it's corresponding user tables
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public void UpdateAgentName(string oldName, string newName)
        {
            _personRepo.ForceIgnoreSecurity();
            Person person = _personRepo.Find(0, 1).Items?.Where(p => p.Name == oldName && p.IsAgent && p.IsDeleted == false)?.FirstOrDefault();
            if (person != null)
            {
                person.Name = newName;
                _personRepo.Update(person);

                _usersRepo.ForceIgnoreSecurity();
                var aspUser = _usersRepo.Find(0, 1).Items?.Where(u => u.PersonId == person.Id)?.FirstOrDefault();
                if (aspUser != null)
                {
                    var existingUser = _userManager.FindByIdAsync(aspUser.Id).Result;
                    existingUser.Name = newName;
                    var result = _userManager.UpdateAsync(existingUser).Result;
                }
                else
                {
                    throw new EntityDoesNotExistException("Could not find the corresponding asp user entity to update");
                }
                _usersRepo.ForceSecurity();
            }
            else
            {
                throw new EntityDoesNotExistException("Could not find the corresponding person entity to update");
            }
        }

        /// <summary>
        /// Gets additional details for the provided agent viewmodel
        /// </summary>
        /// <param name="agentView"></param>
        /// <returns></returns>
        public AgentViewModel GetAgentDetails(AgentViewModel agentView)
        {
            agentView.UserName = _usersRepo.Find(null, u => u.Name == agentView.Name).Items?.FirstOrDefault()?.UserName;
            agentView.CredentialName = _credentialRepo.GetOne(agentView.CredentialId ?? Guid.Empty)?.Name;

            AgentHeartbeat agentHeartBeat = _agentHeartbeatRepo.Find(0, 1).Items?.Where(a => a.AgentId == agentView.Id).OrderByDescending(a => a.CreatedOn).FirstOrDefault();

            if (agentHeartBeat != null)
            {
                agentView.LastReportedOn = agentHeartBeat.LastReportedOn;
                agentView.LastReportedStatus = agentHeartBeat.LastReportedStatus;
                agentView.LastReportedWork = agentHeartBeat.LastReportedWork;
                agentView.LastReportedMessage = agentHeartBeat.LastReportedMessage;
                agentView.IsHealthy = agentHeartBeat.IsHealthy;
            }

            return agentView;
        }

        /// <summary>
        /// Checks if there are any jobs that depend on the specified agent
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool CheckReferentialIntegrity(string id)
        {
            Guid agentId = new Guid(id);

            var scheduleWithAgent = _scheduleRepo.Find(0, 1).Items?
              .Where(s => s.AgentId == agentId);

            var jobWithAgent = _jobRepo.Find(0, 1).Items?
              .Where(j => j.AgentId == agentId && j.JobStatus == JobStatusType.Assigned
              | j.JobStatus == JobStatusType.New
              | j.JobStatus == JobStatusType.InProgress);

            return scheduleWithAgent.Count() > 0 || jobWithAgent.Count() > 0 ? true : false;
        }

        /// <summary>
        /// Gets an enumerable of all agent heartbeats
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns></returns>
        private IEnumerable<AgentHeartbeat> GetAgentHeartbeats(Guid agentId)
        {
            var agentHeartbeats = _agentHeartbeatRepo.Find(0, 1)?.Items?.Where(p => p.AgentId == agentId);
            return agentHeartbeats;
        }

        /// <summary>
        /// Deletes any existing agent heartbeats for the specified agent
        /// </summary>
        /// <param name="agentId"></param>
        private void DeleteExistingHeartbeats(Guid agentId)
        {
            var agentHeartbeats = GetAgentHeartbeats(agentId);
            foreach (var heartbeat in agentHeartbeats)
            {
                _agentHeartbeatRepo.SoftDelete(heartbeat.AgentId);
            }
        }

        /// <summary>
        /// Returns the requested agent if the provided information matches what's stored in the agent
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="requestIp"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public Agent GetConnectAgent(string agentId, string requestIp, ConnectAgentViewModel request)
        {
            Agent agent = _agentRepo.GetOne(Guid.Parse(agentId));
            if (agent == null) return agent;

            if (agent.IsEnhancedSecurity == true)
            {
                if (agent.IPAddresses != requestIp)
                {
                    throw new UnauthorizedOperationException("The IP address provided does not match this agent's IP address", EntityOperationType.Update);
                }
                if (agent.MacAddresses != request.MacAddresses)
                {
                    throw new UnauthorizedOperationException("The MAC address provided does not match this agent's MAC address", EntityOperationType.Update);
                }
            }

            if (agent.MachineName != request.MachineName)
            {
                throw new UnauthorizedOperationException("The machine name provided does not match this agent's machine name", EntityOperationType.Update);
            }

            return agent;
        }
    }
}