using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;

namespace OpenBots.Server.Business
{
    public class AutomationExecutionLogManager : BaseManager, IAutomationExecutionLogManager
    {
        private readonly IAutomationExecutionLogRepository _repo;
        private readonly IAgentRepository _agentRepo;
        private readonly IAutomationRepository _automationRepo;

        public AutomationExecutionLogManager(IAutomationExecutionLogRepository automationExecutionLogRepo, IAgentRepository agentRepo, IAutomationRepository automationRepo)
        {
            _repo = automationExecutionLogRepo;
            _agentRepo = agentRepo;
            _automationRepo = automationRepo;
        }

        public AutomationExecutionViewModel GetExecutionView(AutomationExecutionViewModel executionView)
        {
            executionView.AgentName = _agentRepo.GetOne(executionView.AgentID)?.Name;
            executionView.AutomationName = _automationRepo.GetOne(executionView.AutomationID)?.Name;

            return executionView;
        }

        public PaginatedList<AutomationExecutionViewModel> GetAutomationAndAgentNames(Predicate<AutomationExecutionViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return _repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }
    }
}
