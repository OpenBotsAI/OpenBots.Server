using Hangfire;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;

namespace OpenBots.Server.Business
{
    public class ScheduleManager : BaseManager, IScheduleManager
    {
        private readonly IScheduleRepository repo;
        private readonly IJobRepository jobRepository;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IServiceProvider serviceProvider;

        public ScheduleManager(IScheduleRepository repo, IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider, IJobRepository jobRepository)
        {
            this.repo = repo;
            this.recurringJobManager = recurringJobManager;
            this.serviceProvider = serviceProvider;
            this.jobRepository = jobRepository;
        }

        public PaginatedList<ScheduleViewModel> GetScheduleAgentsandProcesses(Predicate<ScheduleViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }
    }
}
