using System;

namespace OpenBots.Server.ViewModel
{
    public class ScheduleViewModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public Guid? AgentId { get; set; }
        public string? AgentName { get; set; }
        public string? CRONExpression { get; set; }
        public DateTime? LastExecution { get; set; }
        public DateTime? NextExecution { get; set; }
        public bool? IsDisabled { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? ProcessId { get; set; }
        public string? ProcessName { get; set; }
        public string? TriggerName { get; set; }
        public string? StartingType { get; set; }
        public string? Status { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public bool? ScheduleNow { get; set; }
    }
}
