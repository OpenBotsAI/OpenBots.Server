using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model
{
    public class ProcessExecutionLog : Entity
    {
        public Guid JobID { get; set; }
        public Guid ProcessID { get; set; }
        public Guid AgentID { get; set; }
        public DateTime StartedOn { get; set; }
        public DateTime CompletedOn { get; set; }
        public string Trigger { get; set; }
        public string TriggerDetails { get; set; }
        public string Status { get; set; }
        public bool? HasErrors { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetails { get; set; }
    }
}
