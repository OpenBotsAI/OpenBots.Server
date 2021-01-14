using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.Business
{
    public interface IScheduleManager : IManager
    {
        PaginatedList<AllSchedulesViewModel> GetScheduleAgentsandAutomations(Predicate<AllSchedulesViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        void DeleteExistingParameters(Guid scheduleId);
        IEnumerable<ScheduleParameter> GetScheduleParameters(Guid scheduleId);
        PaginatedList<ScheduleParameter> GetScheduleParameters(string scheduleId);
        ScheduleViewModel GetScheduleViewModel(ScheduleViewModel scheduleView);
    }
}