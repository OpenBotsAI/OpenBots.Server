using Hangfire;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.Business.Interfaces;
using System;
using System.Collections.Generic;
using OpenBots.Server.ViewModel;
using System.Text.Json;
using System.Linq;
using Microsoft.AspNetCore.SignalR;

namespace OpenBots.Server.Web.Hubs
{
    public class HubManager : IHubManager
    {
        private readonly IJobRepository _jobRepository;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IAutomationVersionRepository _automationVersionRepository;
        private IHubContext<NotificationHub> _hub;
        private readonly IWebhookPublisher _webhookPublisher;
        private readonly IJobParameterRepository _jobParameterRepository;
        private readonly IScheduleParameterRepository _scheduleParameterRepository;
        private readonly IOrganizationSettingManager _organizationSettingManager;

        public HubManager(IRecurringJobManager recurringJobManager,
            IJobRepository jobRepository, IHubContext<NotificationHub> hub,
            IAutomationVersionRepository automationVersionRepository,
            IWebhookPublisher webhookPublisher,
            IJobParameterRepository jobParameterRepository,
            IScheduleParameterRepository scheduleParameterRepository,
            IOrganizationSettingManager organizationSettingManager)
        {
            _recurringJobManager = recurringJobManager;
            _jobRepository = jobRepository;
            _automationVersionRepository = automationVersionRepository;
            _webhookPublisher = webhookPublisher;
            _jobParameterRepository = jobParameterRepository;
            _hub = hub;
            _scheduleParameterRepository = scheduleParameterRepository;
            _organizationSettingManager = organizationSettingManager;
        }

        public HubManager()
        {
        }

        public void ScheduleNewJob(string scheduleSerializeObject)
        {
            var scheduleObj = JsonSerializer.Deserialize<Schedule>(scheduleSerializeObject);

            if (string.IsNullOrWhiteSpace(scheduleObj.CRONExpression))
            {
                CreateJob(scheduleSerializeObject, Enumerable.Empty<ParametersViewModel>());
            }
            else
            {
                _recurringJobManager.AddOrUpdate(scheduleObj.Id.Value.ToString(), () => CreateJob(scheduleSerializeObject, Enumerable.Empty<ParametersViewModel>()), scheduleObj.CRONExpression);

            }
        }

        public void ExecuteJob(string scheduleSerializeObject, IEnumerable<ParametersViewModel>? parameters)
        {
            CreateJob(scheduleSerializeObject, parameters);
        }

        public string CreateJob(string scheduleSerializeObject, IEnumerable<ParametersViewModel>? parameters)
        {
            var schedule = JsonSerializer.Deserialize<Schedule>(scheduleSerializeObject);

            if (_organizationSettingManager.HasDisallowedExecution())
            {
                return "DisallowedExecution";
            }

            var automationVersion = _automationVersionRepository.Find(null, a => a.AutomationId == schedule.AutomationId).Items?.FirstOrDefault();

            //if this is not a "RunNow" job, then use the schedule parameters
            if (schedule.StartingType.Equals("RunNow") == false)
            {
                List<ParametersViewModel> parametersList = new List<ParametersViewModel>();

                var scheduleParameters = _scheduleParameterRepository.Find(null, p => p.ScheduleId == schedule.Id).Items;
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
                    parametersList.Add(parametersViewModel);
                }
                parameters = parametersList.AsEnumerable();
            }

            Job job = new Job();
            job.AgentId = schedule.AgentId == null ? Guid.Empty : schedule.AgentId.Value;
            job.CreatedBy = schedule.CreatedBy;
            job.CreatedOn = DateTime.UtcNow;
            job.EnqueueTime = DateTime.UtcNow;
            job.JobStatus = JobStatusType.New;
            job.AutomationId = schedule.AutomationId == null ? Guid.Empty : schedule.AutomationId.Value;
            job.AutomationVersion = automationVersion != null ? automationVersion.VersionNumber : 0;
            job.AutomationVersionId = automationVersion != null ? automationVersion.Id : Guid.Empty;
            job.Message = "Job is created through internal system logic.";

            foreach (var parameter in parameters ?? Enumerable.Empty<ParametersViewModel>())
            {
                JobParameter jobParameter = new JobParameter
                {
                    Name = parameter.Name,
                    DataType = parameter.DataType,
                    Value = parameter.Value,
                    JobId = job.Id ?? Guid.Empty,
                    CreatedBy = schedule.CreatedBy,
                    CreatedOn = DateTime.UtcNow,
                    Id = Guid.NewGuid()
                };
                _jobParameterRepository.Add(jobParameter);
            }

            _jobRepository.Add(job);
            _hub.Clients.All.SendAsync("botnewjobnotification", job.AgentId.ToString());
            _webhookPublisher.PublishAsync("Jobs.NewJobCreated", job.Id.ToString()).ConfigureAwait(false);

            return "Success";
        }
    }
}
