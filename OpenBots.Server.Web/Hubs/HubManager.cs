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
using OpenBots.Server.ViewModel;

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
        private readonly IScheduleParameterRepository scheduleParameterRepository;

        public HubManager(IRecurringJobManager recurringJobManager,
            IJobRepository jobRepository, IHubContext<NotificationHub> hub,
            IAutomationVersionRepository automationVersionRepository,
            IWebhookPublisher webhookPublisher,
            IJobParameterRepository jobParameterRepository,
            IScheduleParameterRepository scheduleParameterRepository)
        {
            this.recurringJobManager = recurringJobManager;
            this.jobRepository = jobRepository;
            this.automationVersionRepository = automationVersionRepository;
            this.webhookPublisher = webhookPublisher;
            this.jobParameterRepository = jobParameterRepository;
            _hub = hub;
            this.scheduleParameterRepository = scheduleParameterRepository;
        }

        public HubManager()
        {
        }

        public void ScheduleNewJob(string scheduleSerializeObject)
        {
            List<ParametersViewModel> parameters = new List<ParametersViewModel>();
            var scheduleObj = JsonSerializer.Deserialize<Schedule>(scheduleSerializeObject);

            var scheduleParameters = scheduleParameterRepository.Find(null, p => p.ScheduleId == scheduleObj.Id).Items;
            foreach (var scheduleParameter in scheduleParameters)
            {
                ParametersViewModel parametersViewModel = new ParametersViewModel
                {
                    Name = scheduleParameter.Name,
                    DataType = scheduleParameter.DataType,
                    Value = scheduleParameter.Value,
                    CreatedBy = scheduleParameter.CreatedBy,
                    CreatedOn = DateTime.UtcNow
                };
                parameters.Add(parametersViewModel);
            }

            recurringJobManager.AddOrUpdate(scheduleObj.Id.Value.ToString(), () => CreateJob(scheduleSerializeObject, parameters.AsEnumerable()), scheduleObj.CRONExpression);
        }

        public void ExecuteJob(string scheduleSerializeObject, IEnumerable<ParametersViewModel>? parameters)
        {
            var scheduleObj = JsonSerializer.Deserialize<Schedule>(scheduleSerializeObject);
            CreateJob(scheduleSerializeObject, parameters);
        }

        public string CreateJob(string scheduleSerializeObject, IEnumerable<ParametersViewModel>? parameters)
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

            foreach (var parameter in parameters ?? Enumerable.Empty<ParametersViewModel>())
            {
                JobParameter jobParameter = new JobParameter
                {
                    JobId = job.Id ?? Guid.Empty,
                    CreatedBy = schedule.CreatedBy,
                    CreatedOn = DateTime.UtcNow,
                    Id = Guid.NewGuid()
                };
                jobParameterRepository.Add(jobParameter);
            }

            jobRepository.Add(job);
            _hub.Clients.All.SendAsync("botnewjobnotification", job.AgentId.ToString());
            webhookPublisher.PublishAsync("Jobs.NewJobCreated", job.Id.ToString()).ConfigureAwait(false);

            return "Success";
        }
    }
}
