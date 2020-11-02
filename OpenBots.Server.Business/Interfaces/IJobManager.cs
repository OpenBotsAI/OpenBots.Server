using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.IO;

namespace OpenBots.Server.Business
{
    public interface IJobManager : IManager
    {
        JobViewModel GetJobView(JobViewModel jobView);
        JobsLookupViewModel GetJobAgentsLookup();
        PaginatedList<JobViewModel> GetJobAgentsandProcesses(Predicate<JobViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        NextJobViewModel GetNextJob(Guid agentId);
        string GetCsv(Job[] jobs);
        MemoryStream ZipCsv(FileContentResult csvFile);
    }
}
