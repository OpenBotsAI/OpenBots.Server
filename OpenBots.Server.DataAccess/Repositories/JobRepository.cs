using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class JobRepository : EntityRepository<Job>, IJobRepository
    {
        public JobRepository(StorageContext context, ILogger<Job> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<Job> DbTable()
        {
            return dbContext.Jobs;
        }

        public PaginatedList<AllJobsViewModel> FindAllView(Predicate<AllJobsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<AllJobsViewModel> paginatedList = new PaginatedList<AllJobsViewModel>();
            
            var jobsList = base.Find(null, j => j.IsDeleted == false);
            if (jobsList != null && jobsList.Items != null && jobsList.Items.Count > 0)
            {
                var jobRecord = from j in jobsList.Items
                                join a in dbContext.Agents on j.AgentId equals a.Id into table1
                                from a in table1.DefaultIfEmpty()
                                join p in dbContext.Automations on j.AutomationId equals p.Id into table2
                                from p in table2.DefaultIfEmpty()
                                select new AllJobsViewModel
                                {
                                    Id = j?.Id,
                                    CreatedOn = j?.CreatedOn,
                                    CreatedBy = j?.CreatedBy,
                                    IsSuccessful = j?.IsSuccessful,
                                    Message = j?.Message,
                                    JobStatus = j?.JobStatus,
                                    AgentId = (a == null || a.Id == null) ? Guid.Empty : a.Id.Value,
                                    AgentName = a?.Name,
                                    AutomationId = (p == null || p.Id == null) ? Guid.Empty : p.Id.Value,
                                    AutomationName = p?.Name,
                                    StartTime = j.StartTime,
                                    EndTime = j.EndTime,
                                    EnqueueTime = j.EnqueueTime,
                                    DequeueTime = j.DequeueTime
                                };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        jobRecord = jobRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        jobRecord = jobRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                List<AllJobsViewModel> filterRecord = null;
                if (predicate != null)
                    filterRecord = jobRecord.ToList().FindAll(predicate);
                else
                    filterRecord = jobRecord.ToList();

                paginatedList.Items = filterRecord.Skip(skip).Take(take).ToList();
                
                paginatedList.Completed = jobsList.Completed;
                paginatedList.Impediments = jobsList.Impediments;
                paginatedList.PageNumber = jobsList.PageNumber;
                paginatedList.PageSize = jobsList.PageSize;
                paginatedList.ParentId = jobsList.ParentId;
                paginatedList.Started = jobsList.Started;
                paginatedList.TotalCount = filterRecord?.Count;//jobsList.TotalCount;
            }

            return paginatedList;
        }

        public JobsLookupViewModel GetJobAgentsLookup()
        { 
            JobsLookupViewModel jobsLookup = new JobsLookupViewModel();
            var jobsList = base.Find(null, x => x.IsDeleted == false);
            
            if (jobsList != null && jobsList.Items != null && jobsList.Items.Count > 0)
            {
                var agentRecord = from j in jobsList.Items.GroupBy(j => j.AgentId).Select(j => j.First()).ToList()
                                  join a in dbContext.Agents on j.AgentId equals a.Id into table1
                                  from a in table1.DefaultIfEmpty()                                
                                  select new JobAgentsLookup
                                  {
                                      AgentId = (a == null || a.Id == null) ? Guid.Empty : a.Id.Value,
                                      AgentName = a?.Name                                   
                                  };

                jobsLookup.AgentsLookup = agentRecord.OrderBy(p => p.AgentName).ToList();
                
                var processRecord = from j in jobsList.Items.GroupBy(j => j.AutomationId).Select(j => j.First()).ToList()
                                    join p in dbContext.Automations on j.AutomationId equals p.Id into table2
                                    from p in table2.DefaultIfEmpty()
                                    select new JobAutomationLookup
                                    {
                                        AutomationId = (p == null || p.Id == null) ? Guid.Empty : p.Id.Value,
                                        AutomationName = p?.Name
                                    };

                jobsLookup.AutomationLookup = processRecord.OrderBy(p => p.AutomationName).ToList();
            }
            return jobsLookup;
        }
    }
}
