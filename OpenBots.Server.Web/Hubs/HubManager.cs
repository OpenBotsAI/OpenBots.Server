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

namespace OpenBots.Server.Web.Hubs
{
    public class HubManager : IHubManager
    {
        private readonly IJobRepository jobRepository;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly IAutomationVersionRepository automationVersionRepository;
        private IHubContext<NotificationHub> _hub;
        private readonly IWebhookPublisher webhookPublisher;

        public HubManager(IRecurringJobManager recurringJobManager,
            IJobRepository jobRepository, IHubContext<NotificationHub> hub,
            IAutomationVersionRepository automationVersionRepository,
            IWebhookPublisher webhookPublisher)
        {
            this.recurringJobManager = recurringJobManager;
            this.jobRepository = jobRepository;
            this.automationVersionRepository = automationVersionRepository;
            this.webhookPublisher = webhookPublisher;
            _hub = hub;
        }

        public HubManager()
        {
        }

        public void StartNewRecurringJob(string scheduleSerializeObject)
        {
            var scheduleObj = JsonSerializer.Deserialize<Schedule>(scheduleSerializeObject);

            if (string.IsNullOrWhiteSpace(scheduleObj.CRONExpression))
            {
                CreateJob(scheduleSerializeObject, "30");
            }
            else
            {
                recurringJobManager.AddOrUpdate(scheduleObj.Id.Value.ToString(), () => CreateJob(scheduleSerializeObject, "30"), scheduleObj.CRONExpression);
            }
        }

        public Common.Job CreateCommonJob(string scheduleSerializeObject)
        {
            Common.Job _job = null;
            _job = Common.Job.FromExpression(() => CreateJob(scheduleSerializeObject, "21"));
            return _job;
        }

        public string CreateJob(string scheduleSerializeObject, string jobId = "")
        {
            var schedule = JsonSerializer.Deserialize<Schedule>(scheduleSerializeObject);
            var automationVersion = automationVersionRepository.Find(null, a => a.AutomationId == schedule.AutomationId).Items?.FirstOrDefault();

            Job job = new Job();
            job.AgentId = schedule.AgentId == null ? Guid.Empty : schedule.AgentId.Value;
            job.CreatedBy = schedule.CreatedBy;
            job.CreatedOn = DateTime.Now;
            job.EnqueueTime = DateTime.Now;
            job.JobStatus = JobStatusType.New;
            job.AutomationId = schedule.AutomationId == null ? Guid.Empty : schedule.AutomationId.Value;
            job.AutomationVersion = automationVersion != null? automationVersion.VersionNumber : 0;
            job.AutomationVersionId = automationVersion != null? automationVersion.Id : Guid.Empty;
            job.Message = "Job is created through internal system logic.";

            jobRepository.Add(job);
            _hub.Clients.All.SendAsync("botnewjobnotification", job.AgentId.ToString());
            webhookPublisher.PublishAsync("Jobs.NewJobCreated", job.Id.ToString()).ConfigureAwait(false);

            return "Success";
        }
    }
}
