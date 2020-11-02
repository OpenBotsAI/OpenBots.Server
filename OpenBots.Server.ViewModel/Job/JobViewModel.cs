using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel
{
    public class JobViewModel : IViewModel<Job, JobViewModel>
    {
        public Guid? Id { get; set; }
        public string AgentName { get; set; }
        public string ProcessName { get; set; }        
        public Guid AgentId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? EnqueueTime { get; set; }
        public DateTime? DequeueTime { get; set; }        
        public Guid ProcessId { get; set; }
        public JobStatusType? JobStatus { get; set; }
        public string? Message { get; set; }
        public bool? IsSuccessful { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string ErrorReason { get; set; }
        public string ErrorCode { get; set; }
        public string SerializedErrorString { get; set; }

        public JobViewModel Map(Job entity)
        {
            JobViewModel jobViewModel = new JobViewModel
            {
                Id = entity.Id,
                AgentId = entity.AgentId,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                EnqueueTime = entity.EnqueueTime,
                DequeueTime = entity.DequeueTime,
                JobStatus = entity.JobStatus,
                ProcessId = entity.ProcessId,
                Message = entity.Message,
                IsSuccessful = entity.IsSuccessful,
                CreatedOn = entity.CreatedOn,
                CreatedBy = entity.CreatedBy,
                ErrorReason = entity.ErrorReason,
                ErrorCode = entity.ErrorCode,
                SerializedErrorString = entity.SerializedErrorString        
            };

            return jobViewModel;
        }
    }
}
