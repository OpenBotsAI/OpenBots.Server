using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace OpenBots.Server.Business
{
    public class JobManager : BaseManager, IJobManager
    {
        private readonly IJobRepository repo;
        private readonly IAgentRepository agentRepo;
        private readonly IProcessRepository processRepo;

        public JobManager(IJobRepository repo, IAgentRepository agentRepo, IProcessRepository processRepo)
        {
            this.repo = repo;
            this.agentRepo = agentRepo;
            this.processRepo = processRepo;
        }

        public JobViewModel GetJobView(JobViewModel jobView)
        {
            jobView.AgentName = agentRepo.GetOne(jobView.AgentId)?.Name;
            jobView.ProcessName = processRepo.GetOne(jobView.ProcessId)?.Name;

            return jobView;
        }

        public JobsLookupViewModel GetJobAgentsLookup()
        {
            return repo.GetJobAgentsLookup();
        }

        public PaginatedList<JobViewModel> GetJobAgentsandProcesses(Predicate<JobViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
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

            NextJobViewModel nextJob = new NextJobViewModel()
            {
                IsJobAvailable = job == null ? false : true,
                AssignedJob = job
            };

            return nextJob;
        }

        public string GetCsv(Job[] jobs)
        {
            string csvString = "JobID,Message,IsSuccessful,StartTime,EndTime,EnqueueTime,DequeueTime,JobStatus,AgentID,ProcessID";
            foreach (Job job in jobs)
            {

                csvString += Environment.NewLine + string.Join(",", job.Id, job.Message, job.IsSuccessful, job.StartTime, job.EndTime,
                    job.EnqueueTime, job.DequeueTime, job.JobStatus,job.AgentId, job.ProcessId);
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
