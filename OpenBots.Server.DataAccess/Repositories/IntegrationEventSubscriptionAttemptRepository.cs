using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Webhooks;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class IntegrationEventSubscriptionAttemptRepository : EntityRepository<IntegrationEventSubscriptionAttempt>, IIntegrationEventSubscriptionAttemptRepository
    {
        public IntegrationEventSubscriptionAttemptRepository(StorageContext context, ILogger<IntegrationEventSubscriptionAttempt> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<IntegrationEventSubscriptionAttempt> DbTable()
        {
            return dbContext.IntegrationEventSubscriptionAttempts;
        }

        public PaginatedList<SubscriptionAttemptViewModel> FindAllView(Predicate<SubscriptionAttemptViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<SubscriptionAttemptViewModel> paginatedList = new PaginatedList<SubscriptionAttemptViewModel>();

            var attemptList = base.Find(null, a => a.IsDeleted == false);
            if (attemptList != null && attemptList.Items != null && attemptList.Items.Count > 0)
            {
                var attemptRecord = from a in attemptList.Items
                                join s in dbContext.IntegrationEventSubscriptions on a.IntegrationEventSubscriptionID equals s.Id into table1
                                from s in table1.DefaultIfEmpty()
                                select new SubscriptionAttemptViewModel
                                {
                                    Id = a?.Id,
                                    CreatedOn = a?.CreatedOn,
                                    CreatedBy = a?.CreatedBy,
                                    TransportType = (s == null || s.Id == null) ? null : s.TransportType.ToString(),
                                    EventLogID = a?.EventLogID,
                                    IntegrationEventSubscriptionID = a?.IntegrationEventSubscriptionID,
                                    IntegrationEventName = a.IntegrationEventName,
                                    Status = a?.Status,
                                    AttemptCounter = a.AttemptCounter
                                };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        attemptRecord = attemptRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        attemptRecord = attemptRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                List<SubscriptionAttemptViewModel> filterRecord = null;
                if (predicate != null)
                    filterRecord = attemptRecord.ToList().FindAll(predicate);
                else
                    filterRecord = attemptRecord.ToList();

                paginatedList.Items = filterRecord.Skip(skip).Take(take).ToList();

                paginatedList.Completed = attemptList.Completed;
                paginatedList.Impediments = attemptList.Impediments;
                paginatedList.PageNumber = attemptList.PageNumber;
                paginatedList.PageSize = attemptList.PageSize;
                paginatedList.ParentId = attemptList.ParentId;
                paginatedList.Started = attemptList.Started;
                paginatedList.TotalCount = filterRecord?.Count;
            }

            return paginatedList;
        }
    }
}
