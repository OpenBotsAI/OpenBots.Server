using System;
using System.Collections.Generic;

namespace OpenBots.Server.ViewModel
{
    public class JobsLookupViewModel
    {
        public List<JobAgentsLookup> AgentsLookup { get; set; }
        public List<JobAutomationLookup> AutomationLookup { get; set; }

        public JobsLookupViewModel()
        {
            AgentsLookup = new List<JobAgentsLookup>();
            AutomationLookup = new List<JobAutomationLookup>();
        }
    }

    public class JobAgentsLookup
    {
        public Guid AgentId { get; set; }
        public string AgentName { get; set; }
    }

    public class JobAutomationLookup
    {   public Guid AutomationId { get; set; }
        public string AutomationName { get; set; }
        public string AutomationNameWithVersion { get; set; }
    }
}
