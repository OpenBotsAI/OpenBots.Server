﻿using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model
{
    public class Job : Entity, INonAuditable
    {
        public Guid? ScheduleId { get; set; }
        public Guid? AgentId { get; set; }
        public Guid? AgentGroupId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? ExecutionTimeInMinutes { get; set; }
        public DateTime? EnqueueTime { get; set; }
        public DateTime? DequeueTime { get; set; }
        [Required]
        public Guid? AutomationId { get; set; }
        [Required]
        public int? AutomationVersion { get; set; }
        [Required]
        public Guid? AutomationVersionId { get; set; }
        public JobStatusType? JobStatus { get; set;}
        public string? Message { get; set; }
        public bool? IsSuccessful { get; set; }
        public string ErrorReason { get; set; }
        public string ErrorCode { get; set; }
        public string SerializedErrorString { get; set; } 
        public int AutomationLogCount { get; set; }
        public int AutomationExecutionLogCount { get; set; }
    }

    /// <summary>
    /// Stores the current status of a job
    /// </summary>
    public enum JobStatusType : int
    {
        Unknown = 0,
        New = 1,
        Assigned = 2,
        InProgress = 3,
        Failed = 4,
        Completed = 5,
        Abandoned = 9
    }
}
