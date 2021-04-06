using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel;

namespace OpenBots.Server.Model
{
    public class Schedule : NamedEntity
    {
        public Guid? AgentId { get; set; }
        public Guid? AgentGroupId { get; set; }
        public string? CRONExpression { get; set; }
        public DateTime? LastExecution { get; set; }
        public DateTime? NextExecution { get; set; }
        [DefaultValue(false)]
        public bool? IsDisabled { get; set; }
        public Guid? ProjectId { get; set; }
        public string? TriggerName { get; set; }
        public bool? Recurrence { get; set; }
        public string? StartingType { get; set; }
        public DateTime? StartJobOn { get; set; }
        public DateTime? RecurrenceUnit { get; set; }
        public DateTime? JobRecurEveryUnit { get; set; }
        public DateTime? EndJobOn { get; set; }
        public DateTime? EndJobAtOccurence { get; set; }
        public DateTime? NoJobEndDate { get; set; }
        public string? Status { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? StartDate { get; set; }
        public Guid? AutomationId { get; set; }
        public Guid? QueueId { get; set; }
        [DefaultValue(0)]
        public int? MaxRunningJobs { get; set; }
    }
}
