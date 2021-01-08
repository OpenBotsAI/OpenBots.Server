using Hangfire;
using Common = Hangfire.Common;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using System.Linq;
using OpenBots.Server.Web.Webhooks;
using System.Collections.Generic;

namespace OpenBots.Server.Web.Hubs
{
    public class HubManager : IHubManager
    {
        private readonly IJobRepository jobRepository;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IAutomationVersionRepository automationVersionRepository;
        private IHubContext<NotificationHub> _hub;
        private readonly IWebhookPublisher webhookPublisher;
        private readonly IJobParameterRepository jobParameterRepository;

        public HubManager(IRecurringJobManager recurringJobManager,
            IJobRepository jobRepository, IHubContext<NotificationHub> hub,
            IAutomationVersionRepository automationVersionRepository,
            IWebhookPublisher webhookPublisher,
            IJobParameterRepository jobParameterRepository)
        {
            this.recurringJobManager = recurringJobManager;
            this.jobRepository = jobRepository;
            this.automationVersionRepository = automationVersionRepository;
            this.webhookPublisher = webhookPublisher;
            this.jobParameterRepository = jobParameterRepository;
            _hub = hub;
        }

        public HubManager()
        {
        }

        public void StartNewRecurringJob(string scheduleSerializeObject, IEnumerable<JobParameter>? jobParameters)
        {
            var scheduleObj = JsonSerializer.Deserialize<Schedule>(scheduleSerializeObject);

            if (string.IsNullOrWhiteSpace(scheduleObj.CRONExpression))
            {
                CreateJob(scheduleSerializeObject, jobParameters);
            }
            else
            {
                recurringJobManager.AddOrUpdate(scheduleObj.Id.Value.ToString(), () => CreateJob(scheduleSerializeObject, jobParameters), scheduleObj.CRONExpression);
            }
        }

        public string CreateJob(string scheduleSerializeObject, IEnumerable<JobParameter>? jobParameters)
        {
            var schedule = JsonSerializer.Deserialize<Schedule>(scheduleSerializeObject);
            var automationVersion = automationVersionRepository.Find(null, a => a.AutomationId == schedule.AutomationId).Items?.FirstOrDefault();

            Job job = new Job();
            job.AgentId = schedule.AgentId == null ? Guid.Empty : schedule.AgentId.Value;
            job.CreatedBy = schedule.CreatedBy;
            job.CreatedOn = DateTime.UtcNow;
            job.EnqueueTime = DateTime.UtcNow;
            job.JobStatus = JobStatusType.New;
            job.AutomationId = schedule.AutomationId == null ? Guid.Empty : schedule.AutomationId.Value;
            job.AutomationVersion = automationVersion != null? automationVersion.VersionNumber : 0;
            job.AutomationVersionId = automationVersion != null? automationVersion.Id : Guid.Empty;
            job.Message = "Job is created through internal system logic.";

            foreach (var parameter in jobParameters ?? Enumerable.Empty<JobParameter>())
            {
                parameter.JobId = job.Id ?? Guid.Empty;
                parameter.CreatedBy = schedule.CreatedBy;
                parameter.CreatedOn = DateTime.UtcNow;
                parameter.Id = Guid.NewGuid();
                jobParameterRepository.Add(parameter);
            }

            jobRepository.Add(job);
            _hub.Clients.All.SendAsync("botnewjobnotification", job.AgentId.ToString());
            webhookPublisher.PublishAsync("Jobs.NewJobCreated", job.Id.ToString()).ConfigureAwait(false);

            return "Success";
        }
    }
}
