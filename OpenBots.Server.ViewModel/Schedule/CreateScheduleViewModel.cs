using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class CreateScheduleViewModel : IViewModel<CreateScheduleViewModel, Schedule>
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public Guid? AgentId { get; set; }
        public Guid? AgentGroupId { get; set; }
        public string? CRONExpression { get; set; }
        public string CRONExpressionTimeZone { get; set; }
        public DateTime? LastExecution { get; set; }
        public DateTime? NextExecution { get; set; }
        public bool? IsDisabled { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? AutomationId { get; set; }
        public string? StartingType { get; set; }
        public string? Status { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? StartDate { get; set; }
        public Guid? QueueId { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please enter valid a number greater than or equal to 1")]
        public int? MaxRunningJobs { get; set; } 
        public IEnumerable<ParametersViewModel>? Parameters { get; set; }

        public Schedule Map(CreateScheduleViewModel viewModel)
        {
            Schedule schedule = new Schedule
            {
                Id = viewModel.Id,
                Name = viewModel.Name,
                AgentId = viewModel.AgentId,
                AgentGroupId = viewModel.AgentGroupId,
                CRONExpression = viewModel.CRONExpression,
                CRONExpressionTimeZone = viewModel.CRONExpressionTimeZone,
                LastExecution = viewModel.LastExecution,
                NextExecution = viewModel.NextExecution,
                IsDisabled = viewModel.IsDisabled,
                ProjectId = viewModel.ProjectId,
                AutomationId = viewModel.AutomationId,
                StartingType = viewModel.StartingType,
                Status = viewModel.Status,
                ExpiryDate = viewModel.ExpiryDate,
                StartDate = viewModel.StartDate,
                QueueId = viewModel.QueueId,
                MaxRunningJobs = viewModel.MaxRunningJobs
            };

            return schedule;
        }
    }
}
