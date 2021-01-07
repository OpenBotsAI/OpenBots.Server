using OpenBots.Server.Model;
using System.Collections.Generic;

namespace OpenBots.Server.Web.Hubs
{
    public interface IHubManager
    {
        void StartNewRecurringJob(string scheduleSerializeObject, IEnumerable<JobParameter>? jobParameters);
    }
}
