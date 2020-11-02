using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model
{
    public class ProcessLog: Entity
    { 
        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public string Level { get; set; }
        public DateTime? ProcessLogTimeStamp { get; set; }
        public string Exception { get; set; }
        public string Properties { get; set; }
        public Guid? JobId { get; set; }
        public Guid? ProcessId { get; set; }
        public Guid? AgentId { get; set; }
        public string MachineName { get; set; }
        public string AgentName { get; set; }
        public string ProcessName { get; set; }
        public string Logger { get; set; }
    }
}
