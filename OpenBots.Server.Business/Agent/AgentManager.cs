using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.AgentViewModels;
using OpenBots.Server.Web.Webhooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

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
        private readonly IAgentGroupMemberRepository _agentGroupMemberRepository;
        private readonly IJobManager _jobManager;
        private readonly IWebhookPublisher _webhookPublisher;
        private readonly ClaimsPrincipal _caller;
        private readonly IAgentGroupRepository _agentGroupRepository;
        private readonly IAgentSettingRepository _agentSettingRepository;

        public AgentManager(IAgentRepository agentRepository,
            IScheduleRepository scheduleRepository,
            IJobRepository jobRepository,
            IAspNetUsersRepository usersRepository,
            ICredentialRepository credentialRepository,
            IAgentHeartbeatRepository agentHeartbeatRepository,
            ApplicationIdentityUserManager userManager,
            IPersonRepository personRepository,
            IAgentGroupMemberRepository agentGroupMemberRepository,
            IJobManager jobManager,
            IWebhookPublisher webhookPublisher,
            IHttpContextAccessor httpContextAccessor,
            IAgentGroupRepository agentGroupRepository,
            IAgentSettingRepository agentSettingRepository)
        {
            _agentRepo = agentRepository;
            _scheduleRepo = scheduleRepository;
            _jobRepo = jobRepository;
            _usersRepo = usersRepository;
            _credentialRepo = credentialRepository;
            _agentHeartbeatRepo = agentHeartbeatRepository;
            _userManager = userManager;
            _personRepo = personRepository;
            _agentGroupMemberRepository = agentGroupMemberRepository;
            _jobManager = jobManager;
            _agentGroupRepository = agentGroupRepository;
            _webhookPublisher = webhookPublisher;
            _agentSettingRepository = agentSettingRepository;
            _caller = ((httpContextAccessor.HttpContext != null) ? httpContextAccessor.HttpContext.User : new ClaimsPrincipal());
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

            //create agent setting if one was provided
            if (request.AgentSetting != null)
            {
                AgentSettingViewModel settingViewModel = request.AgentSetting;
                AgentSetting agentSetting = settingViewModel.Map(settingViewModel);
                agentSetting.AgentId = request.Id;

                _agentSettingRepository.Add(agentSetting);
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

            //delete all group members with this agent id
            var allAgentMembers = GetAllMembersInGroup(agent.Id.ToString()).Items;
            foreach (var member in allAgentMembers)
            {
                _agentGroupMemberRepository.SoftDelete(member.Id ?? Guid.Empty);
            }

            DeleteExistingHeartbeats(agent.Id ?? Guid.Empty);

            //delete agent settings
            var agentSettings = _agentSettingRepository.Find(null, s => s.AgentId == agent.Id)?.Items?.FirstOrDefault();
            if (agentSettings != null)
            {
                _agentSettingRepository.SoftDelete(agentSettings.Id.Value);
            }
        }

        /// <summary>
        /// Updates an Agent's user account and returns the updated Agent
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns>Updated Agent</returns>
        public Agent UpdateAgent(string id, UpdateAgentViewModel request)
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

            if (request.AgentSetting != null)
            {
                AgentSettingViewModel settingViewModel = request.AgentSetting;
                AgentSetting agentSetting = settingViewModel.Map(settingViewModel);
                agentSetting.AgentId = entityId;

                var existingSetting = _agentSettingRepository.Find(null, s => s.AgentId == entityId).Items.FirstOrDefault();

                //if setting exists, then update the setting
                if (existingSetting != null)
                {
                    existingSetting.HeartbeatInterval = settingViewModel.HeartbeatInterval;
                    existingSetting.JobLoggingInterval = settingViewModel.JobLoggingInterval;
                    existingSetting.VerifySslCertificate = settingViewModel.VerifySslCertificate;

                    _agentSettingRepository.Update(existingSetting);
                }
                //if setting does not exist, then create a new setting
                else
                {
                    _agentSettingRepository.Add(agentSetting);
                }

            }

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
        /// <returns>Details for the specified Agent</returns>
        public AgentViewModel GetAgentDetails(AgentViewModel agentView)
        {
            agentView.UserName = _usersRepo.Find(null, u => u.Name == agentView.Name).Items?.FirstOrDefault()?.UserName;
            agentView.CredentialName = _credentialRepo.GetOne(agentView.CredentialId ?? Guid.Empty)?.Name;

            //get agent hearbeat details
            AgentHeartbeat agentHeartBeat = _agentHeartbeatRepo.Find(0, 1).Items?.Where(a => a.AgentId == agentView.Id).OrderByDescending(a => a.CreatedOn).FirstOrDefault();
            if (agentHeartBeat != null)
            {
                agentView.LastReportedOn = agentHeartBeat.LastReportedOn;
                agentView.LastReportedStatus = agentHeartBeat.LastReportedStatus;
                agentView.LastReportedWork = agentHeartBeat.LastReportedWork;
                agentView.LastReportedMessage = agentHeartBeat.LastReportedMessage;
                agentView.IsHealthy = agentHeartBeat.IsHealthy;
            }

            //get agent settings details
            AgentSetting agentSetting = _agentSettingRepository.Find(null, s => s.AgentId == agentView.Id).Items.FirstOrDefault();

            if (agentSetting != null)
            {
                AgentSettingViewModel settingViewModel = new AgentSettingViewModel();
                settingViewModel = settingViewModel.MapFromModel(agentSetting);
                agentView.AgentSetting = settingViewModel;
            }

            return agentView;
        }

        /// <summary>
        /// Checks if there are any jobs that depend on the specified agent
        /// </summary>
        /// <param name="id"></param>
        /// <returns>True if the Agent is referenced in dependant entities</returns>
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
        /// Connects the specified Agent and returns Agent details if the provided information matches what's stored in the Agent
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="requestIp"></param>
        /// <param name="request"></param>
        /// <returns>Specified Agent</returns>
        public ConnectedViewModel ConnectAgent(string agentId, string requestIp, ConnectAgentViewModel request)
        {
            Agent agent = _agentRepo.GetOne(Guid.Parse(agentId));
            if (agent == null) throw new EntityDoesNotExistException("No Agent was found for the specified id");

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

            //connect agent if it is not already connected
            if (agent.IsConnected == false)
            {
                agent.IsConnected = true;
                _agentRepo.Update(agent);
            }

            //populate connected view model
            ConnectedViewModel connectedViewModel = new ConnectedViewModel();
            connectedViewModel = connectedViewModel.Map(agent);

            //get agent settings details
            AgentSetting agentSetting = _agentSettingRepository.Find(null, s => s.AgentId == agent.Id).Items.FirstOrDefault();

            if (agentSetting != null)
            {
                AgentSettingViewModel settingViewModel = new AgentSettingViewModel();
                settingViewModel = settingViewModel.MapFromModel(agentSetting);
                connectedViewModel.AgentSetting = settingViewModel;
            }          

            return connectedViewModel;
        }

        /// <summary>
        /// Disconnects the specified Agent if the provided details match what's stored in the Agent
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="requestIp"></param>
        /// <param name="request"></param>
        public void DisconnectAgent(Guid? agentId, string requestIp, ConnectAgentViewModel request)
        {
            Agent agent = _agentRepo.GetOne(agentId.Value);
            if (agent == null) throw new EntityDoesNotExistException("No Agent was found for the specified id");

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

            //disconnect agent if it is already connected
            if (agent.IsConnected == true)
            {
                agent.IsConnected = false;
                _agentRepo.Update(agent);
            }
        }

        /// <summary>
        /// Gets a list of all GroupMembers for the specified Agent
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns>All GroupMembers for the specified Agent</returns>
        public PaginatedList<AgentGroupMember> GetAllMembersInGroup(string agentId)
        {
            var entityId = Guid.Parse(agentId);
            var groupMemberList = _agentGroupMemberRepository.Find(null, a => a.AgentId == entityId);

            return groupMemberList;
        }

        /// <summary>
        /// Creates a new Heartbeat entity for the specified Agent id
        /// </summary>
        /// <param name="request"></param>
        /// <param name="agentId"></param>
        /// <returns>Newly created Heartbeat</returns>
        public AgentHeartbeat PerformAgentHeartbeat(HeartbeatViewModel request, string agentId)
        {
            Agent agent = _agentRepo.GetOne(new Guid(agentId));
            if (agent == null)
            {
                throw new EntityDoesNotExistException("The Agent ID provided does not match any existing Agents");
            }

            if (agent.IsConnected == false)
            {
                throw new EntityOperationException("Agent is not connected. Please connect the agent and try again");
            }

            if (request.IsHealthy == false)
            {
                _webhookPublisher.PublishAsync("Agents.UnhealthyReported", agent.Id.ToString(), agent.Name).ConfigureAwait(false);
            }

            AgentHeartbeat agentHeartbeat = request.Map(request);

            //Add HeartBeat Values
            agentHeartbeat.AgentId = new Guid(agentId);
            agentHeartbeat.CreatedBy = _caller?.Identity.Name;
            agentHeartbeat.CreatedOn = DateTime.UtcNow;
            agentHeartbeat.LastReportedOn = request.LastReportedOn ?? DateTime.UtcNow;
            _agentHeartbeatRepo.Add(agentHeartbeat);

            return agentHeartbeat;
        }

        /// <summary>
        /// Gets the oldest available job for the specified Agent id
        /// </summary>
        /// <param name="agentId"></param>
        /// <returns>Next available Job if one exists</returns>
        public NextJobViewModel GetNextJob(string agentId)
        {
            if (agentId == null)
            { 
                throw new EntityOperationException("No Agent ID was passed");
            }

            bool isValid = Guid.TryParse(agentId, out Guid agentGuid);
            if (!isValid)
            {
                throw new EntityOperationException("Agent ID is not a valid GUID");
            }

            //get all new jobs of the agent group and assign the oldest one to the current agent
            var job = GetNextAgentJob(agentGuid);

            var jobParameters = _jobManager.GetJobParameters(job?.Id ?? Guid.Empty);

            NextJobViewModel nextJob = new NextJobViewModel()
            {
                IsJobAvailable = job == null ? false : true,
                AssignedJob = job,
                JobParameters = jobParameters
            };

            return nextJob;
        }

        /// <summary>
        /// Searches for all Jobs assigned to this Agent or any of its groups
        /// </summary>
        /// <param name="agentGuid"></param>
        /// <returns>The oldest job with a new status</returns>
        public Job GetNextAgentJob(Guid agentGuid)
        {
            List<Job> agentGroupJobs = new List<Job>();
            var agentGroupsMembers = GetAllMembersInGroup(agentGuid.ToString()).Items;
            foreach (var member in agentGroupsMembers)
            {
                AgentGroup agentGroup = _agentGroupRepository.Find(null, g => g.Id == member.AgentGroupId).Items?.FirstOrDefault();
                //if agent group is disabled, then do not retreive jobs from that group
                if (agentGroup.IsEnabled == false) continue;

                var memberGroupJobs = _jobRepo.Find(null, j => j.AgentGroupId == member.AgentGroupId && j.JobStatus == JobStatusType.New).Items;
                agentGroupJobs = agentGroupJobs.Concat(memberGroupJobs).ToList();
            }

            var agentJobs = _jobRepo.Find(null, j => j.AgentId == agentGuid && j.JobStatus == JobStatusType.New).Items;
            var allAgentJobs = agentGroupJobs.Concat(agentJobs).ToList();
            Job job = allAgentJobs.OrderBy(j => j.CreatedOn).FirstOrDefault();

            //update job
            if (job != null)
            {
                job.JobStatus = JobStatusType.Assigned;
                job.DequeueTime = DateTime.UtcNow;
                job.AgentId = agentGuid;
                try
                {
                    job = _jobRepo.Update(job);
                }
                catch (EntityConcurrencyException ex)
                {
                    job = null;
                }
            }
            return job;
        }
    }
}
