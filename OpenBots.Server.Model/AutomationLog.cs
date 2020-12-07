using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model
{
    public class AutomationLog: Entity
    { 
        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public string Level { get; set; }
        public DateTime? AutomationLogTimeStamp { get; set; }
        public string Exception { get; set; }
        public string Properties { get; set; }
        public Guid? JobId { get; set; }
        public Guid? AutomationId { get; set; }
        public Guid? AgentId { get; set; }
        public string MachineName { get; set; }
        public string AgentName { get; set; }
        public string AutomationName { get; set; }
        public string Logger { get; set; }
    }
}
