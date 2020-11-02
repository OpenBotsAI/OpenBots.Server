using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;

namespace OpenBots.Server.Business
{
    public interface IProcessExecutionLogManager : IManager
    {
        ProcessExecutionViewModel GetExecutionView(ProcessExecutionViewModel executionView);
        PaginatedList<ProcessExecutionViewModel> GetProcessAndAgentNames(Predicate<ProcessExecutionViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
    }
}
