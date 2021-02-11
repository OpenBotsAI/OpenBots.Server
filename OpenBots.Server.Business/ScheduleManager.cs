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
        private readonly IScheduleRepository _repo;
        private readonly IScheduleParameterRepository _scheduleParameterRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly IAutomationRepository _automationRepository;

        public ScheduleManager(IScheduleRepository repo, IScheduleParameterRepository scheduleParameterRepository, IAgentRepository agentRepository,
            IAutomationRepository automationRepository)
        {
            _repo = repo;
            _scheduleParameterRepository = scheduleParameterRepository;
            _agentRepository = agentRepository;
            _automationRepository = automationRepository;
        }

        public PaginatedList<AllSchedulesViewModel> GetScheduleAgentsandAutomations(Predicate<AllSchedulesViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return _repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }

        public void DeleteExistingParameters(Guid scheduleId)
        {
            var schedulParameters = GetScheduleParameters(scheduleId);
            foreach (var parmeter in schedulParameters)
            {
                _scheduleParameterRepository.SoftDelete(parmeter.Id ?? Guid.Empty);
            }
        }

        public IEnumerable<ScheduleParameter> GetScheduleParameters(Guid scheduleId)
        {
            var scheduleParameters = _scheduleParameterRepository.Find(0, 1)?.Items?.Where(p => p.ScheduleId == scheduleId);
            return scheduleParameters;
        }

        public PaginatedList<ScheduleParameter> GetScheduleParameters(string scheduleId)
        {
           return _scheduleParameterRepository.Find(null, p => p.ScheduleId == Guid.Parse(scheduleId));
        }

        public ScheduleViewModel GetScheduleViewModel(ScheduleViewModel scheduleView)
        {
            scheduleView.AgentName = _agentRepository.GetOne(scheduleView.AgentId ?? Guid.Empty)?.Name;
            scheduleView.AutomationName = _automationRepository.GetOne(scheduleView.AutomationId ?? Guid.Empty)?.Name;
            scheduleView.ScheduleParameters = GetScheduleParameters(scheduleView.Id ?? Guid.Empty);

            return scheduleView;
        }
    }
}
