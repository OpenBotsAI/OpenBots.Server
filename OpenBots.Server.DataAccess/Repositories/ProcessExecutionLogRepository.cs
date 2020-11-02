using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class ProcessExecutionLogRepository : EntityRepository<ProcessExecutionLog>, IProcessExecutionLogRepository
    {
        public ProcessExecutionLogRepository(StorageContext context, ILogger<ProcessExecutionLog> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<ProcessExecutionLog> DbTable()
        {
            return dbContext.ProcessExecutionLogs;
        }

        public PaginatedList<ProcessExecutionViewModel> FindAllView(Predicate<ProcessExecutionViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<ProcessExecutionViewModel> paginatedList = new PaginatedList<ProcessExecutionViewModel>();

            var processExecutionsList = base.Find(null, e => e.IsDeleted == false);
            if (processExecutionsList != null && processExecutionsList.Items != null && processExecutionsList.Items.Count > 0)
            {
                var processExecutionRecord = from e in processExecutionsList.Items
                                join a in dbContext.Agents on e.AgentID equals a.Id into table1
                                from a in table1.DefaultIfEmpty()
                                join p in dbContext.Processes on e.ProcessID equals p.Id into table2
                                from p in table2.DefaultIfEmpty()
                                select new ProcessExecutionViewModel
                                {
                                    Id = e?.Id,
                                    AgentName = a?.Name,
                                    ProcessName = p?.Name,
                                    JobID = e?.JobID,
                                    ProcessID = (p == null || p.Id == null) ? Guid.Empty : p.Id.Value,
                                    AgentID = (a == null || a.Id == null) ? Guid.Empty : a.Id.Value,
                                    StartedOn = e?.StartedOn,
                                    CompletedOn = e?.CompletedOn,
                                    Trigger = e?.Trigger,
                                    TriggerDetails = e?.TriggerDetails,
                                    Status = e?.Status,
                                    HasErrors = e?.HasErrors,
                                    ErrorMessage = e?.ErrorMessage,
                                    ErrorDetails = e?.ErrorDetails
                                };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        processExecutionRecord = processExecutionRecord.OrderBy(e => e.GetType().GetProperty(sortColumn).GetValue(e)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        processExecutionRecord = processExecutionRecord.OrderByDescending(e => e.GetType().GetProperty(sortColumn).GetValue(e)).ToList();

                List<ProcessExecutionViewModel> filterRecord = null;
                if (predicate != null)
                    filterRecord = processExecutionRecord.ToList().FindAll(predicate);
                else
                    filterRecord = processExecutionRecord.ToList();

                paginatedList.Items = filterRecord.Skip(skip).Take(take).ToList();

                paginatedList.Completed = processExecutionsList.Completed;
                paginatedList.Impediments = processExecutionsList.Impediments;
                paginatedList.PageNumber = processExecutionsList.PageNumber;
                paginatedList.PageSize = processExecutionsList.PageSize;
                paginatedList.ParentId = processExecutionsList.ParentId;
                paginatedList.Started = processExecutionsList.Started;
                paginatedList.TotalCount = filterRecord?.Count;//processExecutionsList.TotalCount;


            }

            return paginatedList;
        }
    }
}
