using System;
using System.Collections.Generic;

namespace OpenBots.Server.ViewModel
{
    public class JobsLookupViewModel
    {
        public List<JobAgentsLookup> AgentsLookup { get; set; }
        public List<JobProcessLookup> ProcessLookup { get; set; }

        public JobsLookupViewModel()
        {
            AgentsLookup = new List<JobAgentsLookup>();
            ProcessLookup = new List<JobProcessLookup>();
        }
    }

    public class JobAgentsLookup
    {
        public Guid AgentId { get; set; }
        public string AgentName { get; set; }
    }

    public class JobProcessLookup
    {   public Guid ProcessId { get; set; }
        public string ProcessName { get; set; }
        public string ProcessNameWithVersion { get; set; }
    }
}
