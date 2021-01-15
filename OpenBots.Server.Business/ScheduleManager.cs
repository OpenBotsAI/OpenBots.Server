using Hangfire;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.Business
{
    public class ScheduleManager : BaseManager, IScheduleManager
    {
        private readonly IScheduleRepository repo;
        private readonly IJobRepository jobRepository;
        private readonly IScheduleParameterRepository scheduleParameterRepository;
        private readonly IAgentRepository agentRepository;
        private readonly IAutomationRepository automationRepository;

        public ScheduleManager(IScheduleRepository repo, IJobRepository jobRepository, IScheduleParameterRepository scheduleParameterRepository, IAgentRepository agentRepository,
            IAutomationRepository automationRepository)
        {
            this.repo = repo;
            this.jobRepository = jobRepository;
            this.scheduleParameterRepository = scheduleParameterRepository;
            this.agentRepository = agentRepository;
            this.automationRepository = automationRepository;
        }

        public PaginatedList<AllSchedulesViewModel> GetScheduleAgentsandAutomations(Predicate<AllSchedulesViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }

        public void DeleteExistingParameters(Guid scheduleId)
        {
            var schedulParameters = GetScheduleParameters(scheduleId);
            foreach (var parmeter in schedulParameters)
            {
                scheduleParameterRepository.SoftDelete(parmeter.Id ?? Guid.Empty);
            }
        }

        public IEnumerable<ScheduleParameter> GetScheduleParameters(Guid scheduleId)
        {
            var scheduleParameters = scheduleParameterRepository.Find(0, 1)?.Items?.Where(p => p.ScheduleId == scheduleId);
            return scheduleParameters;
        }

        public PaginatedList<ScheduleParameter> GetScheduleParameters(string scheduleId)
        {
           return scheduleParameterRepository.Find(null, p => p.ScheduleId == Guid.Parse(scheduleId));
        }

        public ScheduleViewModel GetScheduleViewModel(ScheduleViewModel scheduleView)
        {
            scheduleView.AgentName = agentRepository.GetOne(scheduleView.AgentId ?? Guid.Empty)?.Name;
            scheduleView.AutomationName = automationRepository.GetOne(scheduleView.AutomationId ?? Guid.Empty)?.Name;
            scheduleView.ScheduleParameters = GetScheduleParameters(scheduleView.Id ?? Guid.Empty);

            return scheduleView;
        }
    }
}
