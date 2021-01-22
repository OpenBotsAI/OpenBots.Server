using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.ViewModel
{
    public class ScheduleViewModel : IViewModel<Schedule, ScheduleViewModel>
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
        public Guid? AutomationId { get; set; }
        public string? AutomationName { get; set; }
        public string? TriggerName { get; set; }
        public string? StartingType { get; set; }
        public string? Status { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public bool? ScheduleNow { get; set; }
        public Guid? QueueId { get; set; }
        public IEnumerable<ScheduleParameter>? ScheduleParameters { get; set; }

        public ScheduleViewModel Map(Schedule entity)
        {
            ScheduleViewModel scheduleViewModel = new ScheduleViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                AgentId = entity.AgentId,
                AgentName = entity.AgentName,
                CRONExpression = entity.CRONExpression,
                LastExecution = entity.LastExecution,
                NextExecution = entity.NextExecution,
                IsDisabled = entity.IsDisabled,
                ProjectId = entity.ProjectId,
                AutomationId = entity.AutomationId,
                TriggerName = entity.TriggerName,
                StartingType = entity.StartingType,
                Status = entity.Status,
                ExpiryDate = entity.ExpiryDate,
                StartDate = entity.StartDate,
                CreatedOn = entity.CreatedOn,
                CreatedBy = entity.CreatedBy,
                QueueId = entity.QueueId
            };

            return scheduleViewModel;
        }
    }
}
