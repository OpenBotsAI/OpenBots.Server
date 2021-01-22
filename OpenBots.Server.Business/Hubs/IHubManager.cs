using OpenBots.Server.ViewModel;
using System.Collections.Generic;

namespace OpenBots.Server.Web.Hubs
{
    public interface IHubManager
    {
        void ScheduleNewJob(string scheduleSerializeObject);
        void ExecuteJob(string scheduleSerializeObject, IEnumerable<ParametersViewModel>? parameters);
    }
}
