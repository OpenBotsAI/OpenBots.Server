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
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IServiceProvider serviceProvider;
        private readonly IScheduleParameterRepository scheduleParameterRepository;

        public ScheduleManager(IScheduleRepository repo, IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider, IJobRepository jobRepository, IScheduleParameterRepository scheduleParameterRepository)
        {
            this.repo = repo;
            this.recurringJobManager = recurringJobManager;
            this.serviceProvider = serviceProvider;
            this.jobRepository = jobRepository;
            this.scheduleParameterRepository = scheduleParameterRepository;
        }

        public PaginatedList<ScheduleViewModel> GetScheduleAgentsandAutomations(Predicate<ScheduleViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
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
    }
}
