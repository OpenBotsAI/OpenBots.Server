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
    public class AutomationExecutionLogRepository : EntityRepository<AutomationExecutionLog>, IAutomationExecutionLogRepository
    {
        public AutomationExecutionLogRepository(StorageContext context, ILogger<AutomationExecutionLog> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<AutomationExecutionLog> DbTable()
        {
            return dbContext.AutomationExecutionLogs;
        }

        public PaginatedList<AutomationExecutionViewModel> FindAllView(Predicate<AutomationExecutionViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<AutomationExecutionViewModel> paginatedList = new PaginatedList<AutomationExecutionViewModel>();

            var automationExecutionsList = base.Find(null, e => e.IsDeleted == false);
            if (automationExecutionsList != null && automationExecutionsList.Items != null && automationExecutionsList.Items.Count > 0)
            {
                var automationExecutionRecord = from e in automationExecutionsList.Items
                                join a in dbContext.Agents on e.AgentID equals a.Id into table1
                                from a in table1.DefaultIfEmpty()
                                join p in dbContext.Automations on e.AutomationID equals p.Id into table2
                                from p in table2.DefaultIfEmpty()
                                select new AutomationExecutionViewModel
                                {
                                    Id = e?.Id,
                                    AgentName = a?.Name,
                                    AutomationName = p?.Name,
                                    JobID = e?.JobID,
                                    AutomationID = (p == null || p.Id == null) ? Guid.Empty : p.Id.Value,
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
                        automationExecutionRecord = automationExecutionRecord.OrderBy(e => e.GetType().GetProperty(sortColumn).GetValue(e)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        automationExecutionRecord = automationExecutionRecord.OrderByDescending(e => e.GetType().GetProperty(sortColumn).GetValue(e)).ToList();

                List<AutomationExecutionViewModel> filterRecord = null;
                if (predicate != null)
                    filterRecord = automationExecutionRecord.ToList().FindAll(predicate);
                else
                    filterRecord = automationExecutionRecord.ToList();

                paginatedList.Items = filterRecord.Skip(skip).Take(take).ToList();

                paginatedList.Completed = automationExecutionsList.Completed;
                paginatedList.Impediments = automationExecutionsList.Impediments;
                paginatedList.PageNumber = automationExecutionsList.PageNumber;
                paginatedList.PageSize = automationExecutionsList.PageSize;
                paginatedList.ParentId = automationExecutionsList.ParentId;
                paginatedList.Started = automationExecutionsList.Started;
                paginatedList.TotalCount = filterRecord?.Count;//automationExecutionsList.TotalCount;


            }

            return paginatedList;
        }
    }
}
