using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;

namespace OpenBots.Server.Business
{
    public class AutomationExecutionLogManager : BaseManager, IAutomationExecutionLogManager
    {
        private readonly IAutomationExecutionLogRepository repo;
        private readonly IAgentRepository agentRepo;
        private readonly IAutomationRepository automationRepo;

        public AutomationExecutionLogManager(IAutomationExecutionLogRepository automationExecutionLogRepo, IAgentRepository agentRepo, IAutomationRepository automationRepo)
        {
            this.repo = automationExecutionLogRepo;
            this.agentRepo = agentRepo;
            this.automationRepo = automationRepo;
        }

        public AutomationExecutionViewModel GetExecutionView(AutomationExecutionViewModel executionView)
        {
            executionView.AgentName = agentRepo.GetOne(executionView.AgentID)?.Name;
            executionView.AutomationName = automationRepo.GetOne(executionView.AutomationID)?.Name;

            return executionView;
        }

        public PaginatedList<AutomationExecutionViewModel> GetAutomationAndAgentNames(Predicate<AutomationExecutionViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }
    }
}
