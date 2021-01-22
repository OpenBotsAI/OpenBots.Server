using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class AgentHeartbeatRepository : EntityRepository<AgentHeartbeat>, IAgentHeartbeatRepository
    {
        public AgentHeartbeatRepository(StorageContext context, ILogger<AgentHeartbeat> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<AgentHeartbeat> DbTable()
        {
            return dbContext.AgentHeartbeats;
        }

        public PaginatedList<AgentHeartbeat> FindAllHeartbeats(Guid agentId, Predicate<AgentHeartbeat> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<AgentHeartbeat> paginatedList = new PaginatedList<AgentHeartbeat>();

            var itemsList = base.Find(null, h => h.IsDeleted == false && h.AgentId == agentId);
            if (itemsList != null && itemsList.Items != null && itemsList.Items.Count > 0)
            {
                var itemRecord = from h in itemsList.Items
                                 join a in dbContext.BinaryObjects on h.AgentId equals a.Id into table1
                                 from a in table1.DefaultIfEmpty()
                                 select new AgentHeartbeat
                                 {
                                     AgentId = h.AgentId,
                                     LastReportedOn = h?.LastReportedOn,
                                     LastReportedStatus = h?.LastReportedStatus,
                                     LastReportedWork = h?.LastReportedWork,
                                     LastReportedMessage = h?.LastReportedMessage,
                                     IsHealthy = h?.IsHealthy,
                                     Id = h?.Id,
                                     IsDeleted = h?.IsDeleted,
                                     CreatedBy = h?.CreatedBy,
                                     CreatedOn = h?.CreatedOn,
                                     DeletedBy = h?.DeletedBy,
                                     DeleteOn = h?.DeleteOn,
                                     Timestamp = h?.Timestamp,
                                     UpdatedOn = h?.UpdatedOn,
                                     UpdatedBy = h?.UpdatedBy
                                 };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        itemRecord = itemRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        itemRecord = itemRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                List<AgentHeartbeat> filterRecord = null;
                if (predicate != null)
                    filterRecord = itemRecord.ToList().FindAll(predicate);
                else
                    filterRecord = itemRecord.ToList();

                paginatedList.Items = filterRecord.Skip(skip).Take(take).ToList();


                if (skip == 0 || take == 0)
                {
                    paginatedList.PageNumber = 0;
                    paginatedList.PageSize = 0;
                }
                else
                {
                    int pageNumber = skip;
                    if (skip != 0 && take != 0)
                        pageNumber = skip / take;

                    paginatedList.PageNumber = pageNumber;
                    paginatedList.PageSize = take;
                }
                paginatedList.Completed = itemsList.Completed;
                paginatedList.Impediments = itemsList.Impediments;
                paginatedList.ParentId = itemsList.ParentId;
                paginatedList.Started = itemsList.Started;
                paginatedList.TotalCount = filterRecord?.Count;
            }

            return paginatedList;
        }
    }
}
