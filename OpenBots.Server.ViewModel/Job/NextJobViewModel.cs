using OpenBots.Server.Model;
using System.Collections.Generic;

namespace OpenBots.Server.ViewModel
{
    public class NextJobViewModel
    {
        public bool IsJobAvailable { get; set; }
        public Job AssignedJob { get; set; }
        public IEnumerable<JobParameter> JobParameters { get; set; }
    }
}
