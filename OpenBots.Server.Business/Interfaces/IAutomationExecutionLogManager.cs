using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;

namespace OpenBots.Server.Business
{
    public interface IAutomationExecutionLogManager : IManager
    {
        AutomationExecutionViewModel GetExecutionView(AutomationExecutionViewModel executionView);
        PaginatedList<AutomationExecutionViewModel> GetAutomationAndAgentNames(Predicate<AutomationExecutionViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
    }
}
