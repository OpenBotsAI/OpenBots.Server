using Hangfire;
using Common = Hangfire.Common;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace OpenBots.Server.Web.Hubs
{
    public class HubManager : IHubManager
    {
        private readonly IJobRepository jobRepository;
        private readonly IRecurringJobManager recurringJobManager;
        private IHubContext<NotificationHub> _hub;

        public HubManager(IRecurringJobManager recurringJobManager,
            IJobRepository jobRepository, IHubContext<NotificationHub> hub)
        {
            this.recurringJobManager = recurringJobManager;
            this.jobRepository = jobRepository;
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

            Job job = new Job();
            job.AgentId = schedule.AgentId == null ? Guid.Empty : schedule.AgentId.Value;
            job.CreatedBy = schedule.CreatedBy;
            job.CreatedOn = DateTime.Now;
            job.EnqueueTime = DateTime.Now;
            job.JobStatus = JobStatusType.New;
            job.ProcessId = schedule.ProcessId == null ? Guid.Empty : schedule.ProcessId.Value;
            job.Message = "Job is created through internal system logic.";

            jobRepository.Add(job);
            _hub.Clients.All.SendAsync("botnewjobnotification", job.AgentId.ToString());

            return "Success";
        }
    }
}
