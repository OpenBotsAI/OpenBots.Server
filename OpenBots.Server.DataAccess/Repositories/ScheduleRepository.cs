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
    /// <summary>
    /// Schedule Repository
    /// </summary>
    public class ScheduleRepository : EntityRepository<Schedule>, IScheduleRepository
    {
        /// <summary>
        /// Constructor for Schedule Repository
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        /// <param name="httpContextAccessor"></param>
        public ScheduleRepository(StorageContext context, ILogger<Schedule> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<Schedule> DbTable()
        {
            return dbContext.Schedules;
        }

        public PaginatedList<ScheduleViewModel> FindAllView(Predicate<ScheduleViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<ScheduleViewModel> paginatedList = new PaginatedList<ScheduleViewModel>();

            var schedulesList = base.Find(null, j => j.IsDeleted == false);
            if (schedulesList != null && schedulesList.Items != null && schedulesList.Items.Count > 0)
            {
                var scheduleRecord = from s in schedulesList.Items
                                join a in dbContext.Agents on s.AgentId equals a.Id into table1
                                from a in table1.DefaultIfEmpty()
                                join p in dbContext.Processes on s.ProcessId equals p.Id into table2
                                from p in table2.DefaultIfEmpty()
                                select new ScheduleViewModel
                                {
                                    Id = s?.Id,
                                    StartDate = s?.StartDate,
                                    ExpiryDate = s?.ExpiryDate,
                                    Name = s?.Name,
                                    StartingType = s?.StartingType,
                                    Status = s?.Status,
                                    AgentId = (a == null || a.Id == null) ? Guid.Empty : a.Id.Value,
                                    AgentName = a?.Name,
                                    ProcessId = (p == null || p.Id == null) ? Guid.Empty : p.Id.Value,
                                    ProcessName = p?.Name,
                                    CRONExpression = s?.CRONExpression,
                                    IsDisabled = s?.IsDisabled,
                                    NextExecution = s?.NextExecution,
                                    LastExecution = s?.LastExecution,
                                    TriggerName = s?.TriggerName,
                                    ProjectId = s?.ProjectId,
                                    CreatedOn = s?.CreatedOn,
                                    CreatedBy = s?.CreatedBy
                                };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        scheduleRecord = scheduleRecord.OrderBy(s => s.GetType().GetProperty(sortColumn).GetValue(s)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        scheduleRecord = scheduleRecord.OrderByDescending(s => s.GetType().GetProperty(sortColumn).GetValue(s)).ToList();

                List<ScheduleViewModel> filterRecord = null;
                if (predicate != null)
                    filterRecord = scheduleRecord.ToList().FindAll(predicate);
                else
                    filterRecord = scheduleRecord.ToList();

                paginatedList.Items = filterRecord.Skip(skip).Take(take).ToList();

                paginatedList.Completed = schedulesList.Completed;
                paginatedList.Impediments = schedulesList.Impediments;
                paginatedList.PageNumber = schedulesList.PageNumber;
                paginatedList.PageSize = schedulesList.PageSize;
                paginatedList.ParentId = schedulesList.ParentId;
                paginatedList.Started = schedulesList.Started;
                paginatedList.TotalCount = filterRecord?.Count;


            }

            return paginatedList;
        }
    }
}
