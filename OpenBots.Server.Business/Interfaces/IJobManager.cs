using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenBots.Server.Business
{
    public interface IJobManager : IManager
    {
        Job UpdateJob(string id, CreateJobViewModel request, ApplicationUser applicationUser);
        JobViewModel GetJobView(JobViewModel jobView);
        JobsLookupViewModel GetJobAgentsLookup();
        PaginatedList<AllJobsViewModel> GetJobAgentsandAutomations(Predicate<AllJobsViewModel> predicate = null,
            string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        IEnumerable<JobCheckpoint> GetJobCheckpoints(Guid jobId);
        void DeleteExistingCheckpoints(Guid jobId);
        string GetCsv(Job[] jobs);
        MemoryStream ZipCsv(FileContentResult csvFile);
        void UpdateAutomationAverages(Guid? updatedJobId);
        void DeleteJobChildTables(Guid jobId);
        IEnumerable<JobParameter> UpdateJobParameters(IEnumerable<JobParameter> jobParameters, Guid? jobId);
        IEnumerable<JobParameter> GetJobParameters(Guid? jobId);
        IEnumerable<JobParameter> AddJobParameters(IEnumerable<JobParameter> jobParameters, Guid? jobId);
        void DeleteExistingParameters(Guid? jobId);
        void CheckParameterNameAvailability(IEnumerable<JobParameter> jobParameters);

        object GetJobTotals(List<Job> jobs);
    }
}
