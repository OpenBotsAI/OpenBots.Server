﻿using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.ViewModel
{
    public class AllJobsViewModel : IViewModel<Job, AllJobsViewModel>
    {
        public Guid? Id { get; set; }
        public string AgentName { get; set; }
        public string ProcessName { get; set; }
        [Required]
        public Guid? AgentId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? EnqueueTime { get; set; }
        public DateTime? DequeueTime { get; set; }
        [Required]
        public Guid? ProcessId { get; set; }
        [Required]
        public int? ProcessVersion { get; set; }
        [Required]
        public Guid? ProcessVersionId { get; set; }
        public JobStatusType? JobStatus { get; set; }
        public string? Message { get; set; }
        public bool? IsSuccessful { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string ErrorReason { get; set; }
        public string ErrorCode { get; set; }
        public string SerializedErrorString { get; set; }

        public AllJobsViewModel Map(Job entity)
        {
            AllJobsViewModel jobViewModel = new AllJobsViewModel
            {
                Id = entity.Id,
                AgentId = entity.AgentId,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                EnqueueTime = entity.EnqueueTime,
                DequeueTime = entity.DequeueTime,                
                ProcessId = entity.ProcessId,
                ProcessVersion = entity.ProcessVersion,
                ProcessVersionId = entity.ProcessVersionId,
                JobStatus = entity.JobStatus,
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