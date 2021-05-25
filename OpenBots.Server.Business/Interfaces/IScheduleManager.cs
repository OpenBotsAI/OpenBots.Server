using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.Business
{
    public interface IScheduleManager : IManager
    {
        Schedule AddSchedule(CreateScheduleViewModel schedule);
        Schedule UpdateSchedule(string id, CreateScheduleViewModel request);
        void AttemptPatchUpdate(JsonPatchDocument<Schedule> request, string id);
        PaginatedList<AllSchedulesViewModel> GetScheduleAgentsandAutomations(Predicate<AllSchedulesViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        void DeleteExistingParameters(string scheduleId);
        IEnumerable<ScheduleParameter> GetScheduleParameters(Guid scheduleId);
        PaginatedList<ScheduleParameter> GetScheduleParameters(string scheduleId);
        ScheduleViewModel GetScheduleViewModel(ScheduleViewModel scheduleView);
        string GetTimeZoneId(string cronExpressionTimeZone);
    }
}