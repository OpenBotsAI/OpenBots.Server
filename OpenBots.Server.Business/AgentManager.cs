using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.Business
{
    public class AgentManager : BaseManager, IAgentManager
    {
        private readonly IAgentRepository agentRepo;
        private readonly IScheduleRepository scheduleRepo;
        private readonly IJobRepository jobRepo;
        private readonly IAspNetUsersRepository usersRepo;
        private readonly ICredentialRepository credentialRepo;
        private readonly IAgentHeartbeatRepository agentHeartbeatRepo;

        public AgentManager(IAgentRepository agentRepository, IScheduleRepository scheduleRepository, IJobRepository jobRepository, IAspNetUsersRepository usersRepository,
            ICredentialRepository credentialRepository, IAgentHeartbeatRepository agentHeartbeatRepository)
        {
            this.agentRepo = agentRepository;
            this.scheduleRepo = scheduleRepository;
            this.jobRepo = jobRepository;
            this.usersRepo = usersRepository;
            this.credentialRepo = credentialRepository;
            this.agentHeartbeatRepo = agentHeartbeatRepository;
        }

        public AgentViewModel GetAgentDetails(AgentViewModel agentView)
        {
            agentView.UserName = usersRepo.Find(null, u => u.Name == agentView.Name).Items?.FirstOrDefault()?.UserName;
            agentView.CredentialName = credentialRepo.GetOne(agentView.CredentialId??Guid.Empty)?.Name;

            AgentHeartbeat agentHeartBeat = agentHeartbeatRepo.Find(0,1).Items?.Where(a=>a.AgentId == agentView.Id).OrderByDescending(a=>a.CreatedOn).FirstOrDefault();

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

        public bool CheckReferentialIntegrity(string id)
        {
            Guid agentId = new Guid(id);

            var scheduleWithAgent = scheduleRepo.Find(0, 1).Items?
              .Where(s => s.AgentId == agentId);

            var jobWithAgent = jobRepo.Find(0, 1).Items?
              .Where(j => j.AgentId == agentId && j.JobStatus == JobStatusType.Assigned 
              | j.JobStatus == JobStatusType.New
              | j.JobStatus == JobStatusType.InProgress);

            return scheduleWithAgent.Count() > 0 || jobWithAgent.Count() > 0 ? true : false;
        }

        public IEnumerable<AgentHeartbeat> GetAgentHeartbeats(Guid agentId)
        {
            var agentHeartbeats = agentHeartbeatRepo.Find(0, 1)?.Items?.Where(p => p.AgentId == agentId);
            return agentHeartbeats;
        }

        public void DeleteExistingHeartbeats(Guid agentId)
        {
            var agentHeartbeats = GetAgentHeartbeats(agentId);
            foreach (var heartbeat in agentHeartbeats)
            {
                agentHeartbeatRepo.SoftDelete(heartbeat.AgentId);
            }

        }
    }
}
