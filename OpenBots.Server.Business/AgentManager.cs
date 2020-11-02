using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using System;
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

        public AgentManager(IAgentRepository agentRepository, IScheduleRepository scheduleRepository, IJobRepository jobRepository, IAspNetUsersRepository usersRepository,
            ICredentialRepository credentialRepository)
        {
            this.agentRepo = agentRepository;
            this.scheduleRepo = scheduleRepository;
            this.jobRepo = jobRepository;
            this.usersRepo = usersRepository;
            this.credentialRepo = credentialRepository;
        }

        public AgentViewModel GetAgentDetails(AgentViewModel agentView)
        {
            agentView.UserName = usersRepo.Find(null, u => u.Name == agentView.Name).Items?.FirstOrDefault()?.UserName;
            agentView.CredentialName = credentialRepo.GetOne(agentView.CredentialId??Guid.Empty)?.Name;

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
    }
}
