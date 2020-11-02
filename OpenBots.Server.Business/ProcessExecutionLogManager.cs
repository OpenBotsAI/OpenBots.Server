using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;

namespace OpenBots.Server.Business
{
    public class ProcessExecutionLogManager : BaseManager, IProcessExecutionLogManager
    {
        private readonly IProcessExecutionLogRepository repo;
        private readonly IAgentRepository agentRepo;
        private readonly IProcessRepository processRepo;

        public ProcessExecutionLogManager(IProcessExecutionLogRepository processExecutionLogRepo, IAgentRepository agentRepo, IProcessRepository processRepo)
        {
            this.repo = processExecutionLogRepo;
            this.agentRepo = agentRepo;
            this.processRepo = processRepo;
        }

        public ProcessExecutionViewModel GetExecutionView(ProcessExecutionViewModel executionView)
        {
            executionView.AgentName = agentRepo.GetOne(executionView.AgentID)?.Name;
            executionView.ProcessName = processRepo.GetOne(executionView.ProcessID)?.Name;

            return executionView;
        }

        public PaginatedList<ProcessExecutionViewModel> GetProcessAndAgentNames(Predicate<ProcessExecutionViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }
    }
}
