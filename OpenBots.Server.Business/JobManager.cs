using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace OpenBots.Server.Business
{
    public class JobManager : BaseManager, IJobManager
    {
        private readonly IJobRepository repo;
        private readonly IAgentRepository agentRepo;
        private readonly IAutomationRepository automationRepo;
        private readonly IJobParameterRepository jobParameterRepo;
        private readonly IJobCheckpointRepository jobCheckpointRepo;

        public JobManager(IJobRepository repo, IAgentRepository agentRepo, IAutomationRepository automationRepo,
            IJobParameterRepository jobParameterRepository, IJobCheckpointRepository jobCheckpointRepository)
        {
            this.repo = repo;
            this.agentRepo = agentRepo;
            this.automationRepo = automationRepo;
            this.jobParameterRepo = jobParameterRepository;
            this.jobCheckpointRepo = jobCheckpointRepository;
        }

        public JobViewModel GetJobView(JobViewModel jobView)
        {
            jobView.AgentName = agentRepo.GetOne(jobView.AgentId ?? Guid.Empty)?.Name;
            jobView.AutomationName = automationRepo.GetOne(jobView.AutomationId ?? Guid.Empty)?.Name;
            jobView.JobParameters = GetJobParameters(jobView.Id ?? Guid.Empty);

            return jobView;
        }

        public JobsLookupViewModel GetJobAgentsLookup()
        {
            return repo.GetJobAgentsLookup();
        }

        public PaginatedList<AllJobsViewModel> GetJobAgentsandAutomations(Predicate<AllJobsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }

        //Gets the next available job for the given agentId
        public NextJobViewModel GetNextJob(Guid agentId)
        {
            Job job = repo.Find(0, 1).Items
              .Where(j => j.AgentId == agentId && j.JobStatus == JobStatusType.New)
              .OrderBy(j => j.CreatedOn)
              .FirstOrDefault();

            var jobParameters = GetJobParameters(job?.Id ?? Guid.Empty);

            NextJobViewModel nextJob = new NextJobViewModel()
            {
                IsJobAvailable = job == null ? false : true,
                AssignedJob = job,
                JobParameters = jobParameters
            };

            return nextJob;
        }

        public IEnumerable<JobParameter> GetJobParameters(Guid jobId)
        {
            var jobParameters = jobParameterRepo.Find(0, 1)?.Items?.Where(p => p.JobId == jobId);
            return jobParameters;
        }

        public IEnumerable<JobCheckpoint> GetJobCheckpoints(Guid jobId)
        {
            var jobCheckPoints = jobCheckpointRepo.Find(0, 1)?.Items?.Where(p => p.JobId == jobId);
            return jobCheckPoints;
        }

        public void DeleteExistingParameters(Guid jobId)
        {
            var jobParameters = GetJobParameters(jobId);
            foreach (var parmeter in jobParameters)
            {
                jobParameterRepo.SoftDelete(parmeter.Id ?? Guid.Empty);
            }
        }

        public void DeleteExistingCheckpoints(Guid jobId)
        {
            var jobCheckpoints = GetJobCheckpoints(jobId);
            foreach (var checkpoint in jobCheckpoints)
            {
                jobCheckpointRepo.SoftDelete(checkpoint.Id ?? Guid.Empty);
            }
        }

        public string GetCsv(Job[] jobs)
        {
            string csvString = "JobID,Message,IsSuccessful,StartTime,EndTime,EnqueueTime,DequeueTime,JobStatus,AgentID,AutomationID";
            foreach (Job job in jobs)
            {

                csvString += Environment.NewLine + string.Join(",", job.Id, job.Message, job.IsSuccessful, job.StartTime, job.EndTime,
                    job.EnqueueTime, job.DequeueTime, job.JobStatus,job.AgentId, job.AutomationId);
            }

            return csvString;
        }

        public MemoryStream ZipCsv(FileContentResult csvFile)
        {
            var compressedFileStream = new MemoryStream();

            using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Update))
            {
                var zipEntry = zipArchive.CreateEntry("Jobs.csv");

                using (var originalFileStream = new MemoryStream(csvFile.FileContents))
                using (var zipEntryStream = zipEntry.Open())
                {
                    originalFileStream.CopyTo(zipEntryStream);
                }
            }
            return compressedFileStream;
        }
    }
}
