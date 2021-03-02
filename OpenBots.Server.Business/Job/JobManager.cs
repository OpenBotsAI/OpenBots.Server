using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
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
        private readonly IJobRepository _repo;
        private readonly IAgentRepository _agentRepo;
        private readonly IAutomationRepository _automationRepo;
        private readonly IJobParameterRepository _jobParameterRepo;
        private readonly IJobCheckpointRepository _jobCheckpointRepo;
        private readonly IAutomationVersionRepository _automationVersionRepo;

        public JobManager(IJobRepository jobRepository, 
            IAgentRepository agentRepository,
            IAutomationRepository automationRepository,
            IJobParameterRepository jobParameterRepository,
            IJobCheckpointRepository jobCheckpointRepository,
            IAutomationVersionRepository automationVersionRepository)
        {
            _repo = jobRepository;
            _agentRepo = agentRepository;
            _automationRepo = automationRepository;
            _jobParameterRepo = jobParameterRepository;
            _jobCheckpointRepo = jobCheckpointRepository;
            _automationVersionRepo = automationVersionRepository;
        }

        public Job UpdateJob(string id, CreateJobViewModel request, ApplicationUser applicationUser)
        {
            Guid entityId = new Guid(id);

            var existingJob = _repo.GetOne(entityId);
            if (existingJob == null) throw new EntityDoesNotExistException("Unable to find a job for the specified id");

            Automation automation = _automationRepo.GetOne(existingJob.AutomationId ?? Guid.Empty);
            if (automation == null) //no automation was found
            {
                throw new EntityDoesNotExistException("No automation was found for the specified automation id"); 
            }

            AutomationVersion automationVersion = _automationVersionRepo.Find(null, q => q.AutomationId == automation.Id).Items?.FirstOrDefault();
            existingJob.AutomationVersion = automationVersion.VersionNumber;
            existingJob.AutomationVersionId = automationVersion.Id;

            existingJob.AgentId = request.AgentId;
            existingJob.StartTime = request.StartTime;
            existingJob.EndTime = request.EndTime;
            existingJob.ExecutionTimeInMinutes = (existingJob.EndTime.Value - existingJob.StartTime).Value.TotalMinutes;
            existingJob.DequeueTime = request.DequeueTime;
            existingJob.AutomationId = request.AutomationId;
            existingJob.JobStatus = request.JobStatus;
            existingJob.Message = request.Message;
            existingJob.IsSuccessful = request.IsSuccessful;

            DeleteExistingParameters(entityId);

            var set = new HashSet<string>();
            foreach (var parameter in request.JobParameters ?? Enumerable.Empty<JobParameter>())
            {
                if (!set.Add(parameter.Name))
                {
                    throw new Exception("Job parameter name already exists");
                }
                parameter.JobId = entityId;
                parameter.CreatedBy = applicationUser?.UserName;
                parameter.CreatedOn = DateTime.UtcNow;
                parameter.Id = Guid.NewGuid();
                _jobParameterRepo.Add(parameter);
            }

            if (request.EndTime != null)
            {
                UpdateAutomationAverages(existingJob.Id);
            }

            return existingJob;
        }

        public JobViewModel GetJobView(JobViewModel jobView)
        {
            jobView.AgentName = _agentRepo.GetOne(jobView.AgentId ?? Guid.Empty)?.Name;
            jobView.AutomationName = _automationRepo.GetOne(jobView.AutomationId ?? Guid.Empty)?.Name;
            jobView.JobParameters = GetJobParameters(jobView.Id ?? Guid.Empty);

            return jobView;
        }

        public JobsLookupViewModel GetJobAgentsLookup()
        {
            return _repo.GetJobAgentsLookup();
        }

        public PaginatedList<AllJobsViewModel> GetJobAgentsandAutomations(Predicate<AllJobsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return _repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }

        public IEnumerable<JobParameter> GetJobParameters(Guid jobId)
        {
            var jobParameters = _jobParameterRepo.Find(0, 1)?.Items?.Where(p => p.JobId == jobId);
            return jobParameters;
        }

        public IEnumerable<JobCheckpoint> GetJobCheckpoints(Guid jobId)
        {
            var jobCheckPoints = _jobCheckpointRepo.Find(0, 1)?.Items?.Where(p => p.JobId == jobId);
            return jobCheckPoints;
        }

        public void DeleteExistingParameters(Guid jobId)
        {
            var jobParameters = GetJobParameters(jobId);
            foreach (var parmeter in jobParameters)
            {
                _jobParameterRepo.SoftDelete(parmeter.Id ?? Guid.Empty);
            }
        }

        public void DeleteExistingCheckpoints(Guid jobId)
        {
            var jobCheckpoints = GetJobCheckpoints(jobId);
            foreach (var checkpoint in jobCheckpoints)
            {
                _jobCheckpointRepo.SoftDelete(checkpoint.Id ?? Guid.Empty);
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

        //updates the automation averages for the specified job's automation
        public void UpdateAutomationAverages(Guid? updatedJobId)
        {
            Job updatedJob = _repo.GetOne(updatedJobId ?? Guid.Empty);
            Automation automation = _automationRepo.Find(null, a => a.Id == updatedJob.AutomationId).Items.FirstOrDefault();
            List<Job> sameAutomationJobs;


            if (updatedJob.IsSuccessful ?? false)
            {
                sameAutomationJobs = _repo.Find(null, j => j.AutomationId == automation.Id && j.IsSuccessful == true).Items;
                automation.AverageSuccessfulExecutionInMinutes = GetAverageExecutionTime(sameAutomationJobs);
            }
            else
            {
                sameAutomationJobs = _repo.Find(null, j => j.AutomationId == automation.Id && j.IsSuccessful == false).Items;
                automation.AverageUnSuccessfulExecutionInMinutes = GetAverageExecutionTime(sameAutomationJobs);
            }

            _automationRepo.Update(automation);
        }

        //gets the average execution time for the provided jobs
        public double? GetAverageExecutionTime(List<Job> sameAutomationJobs)
        {
            double? sum = 0;

            foreach (var job in sameAutomationJobs)
            {
                sum += job.ExecutionTimeInMinutes;
            }

            return sum / sameAutomationJobs.Count; 
        }

        public void DeleteJobChildTables(Guid jobId)
        {
            var existingJob = _repo.GetOne(jobId);

            if (existingJob == null)
            {
                throw new EntityDoesNotExistException("Job cannot be found or does not exist.");
            }

            if (existingJob.JobStatus == JobStatusType.InProgress)
            {
                throw new UnauthorizedOperationException("In-Progress jobs cannot be deleted. Please wait for the job to be completed ", EntityOperationType.Delete);
            }

            DeleteExistingParameters(jobId);
            DeleteExistingCheckpoints(jobId);
        }
    }
}
