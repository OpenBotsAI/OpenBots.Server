using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class CreateJobViewModel : IViewModel<CreateJobViewModel, Job>
    {
        public Guid? Id { get; set; }
        [Required]
        public Guid? AgentId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? EnqueueTime { get; set; }
        public DateTime? DequeueTime { get; set; }
        [Required]
        public Guid? AutomationId { get; set; }
        public JobStatusType? JobStatus { get; set; }
        public string? Message { get; set; }
        public bool? IsSuccessful { get; set; }
        public IEnumerable<JobParameter>? JobParameters { get; set; }

        public Job Map(CreateJobViewModel viewModel)
        {
            Job job = new Job
            {
                Id = viewModel.Id,
                AgentId = viewModel.AgentId,
                StartTime = viewModel.StartTime,
                EndTime = viewModel.EndTime,
                EnqueueTime = viewModel.EnqueueTime,
                DequeueTime = viewModel.DequeueTime,
                AutomationId = viewModel.AutomationId,
                JobStatus = viewModel.JobStatus,
                Message = viewModel.Message,
                IsSuccessful = viewModel.IsSuccessful
            };

            return job;
        }
    }
}
