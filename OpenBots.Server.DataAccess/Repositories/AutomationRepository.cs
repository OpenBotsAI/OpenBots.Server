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
    /// Automation Repository
    /// </summary>
    public class AutomationRepository : EntityRepository<Automation>, IAutomationRepository
    {
        /// <summary>
        /// Construtor for Automation Repository
        /// </summary>
        /// <param name="storageContext"></param>
        /// <param name="logger"></param>
        /// <param name="httpContextAccessor"></param>
        public AutomationRepository(StorageContext storageContext, ILogger<Automation> logger, IHttpContextAccessor httpContextAccessor) :base(storageContext, logger, httpContextAccessor) 
        {
        }

        /// <summary>
        /// Retrieves automationes
        /// </summary>
        /// <returns></returns>
        protected override DbSet<Automation> DbTable()
        {
            return DbContext.Automations;
        }

        public PaginatedList<AllAutomationsViewModel> FindAllView(Predicate<AllAutomationsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<AllAutomationsViewModel> paginatedList = new PaginatedList<AllAutomationsViewModel>();

            var itemsList = base.Find(null, j => j.IsDeleted == false);
            if (itemsList != null && itemsList.Items != null && itemsList.Items.Count > 0)
            {
                var itemRecord = from p in itemsList.Items
                                 join v in dbContext.AutomationVersions on p.Id equals v.AutomationId into table1
                                 from v in table1.DefaultIfEmpty()
                                 select new AllAutomationsViewModel
                                 {
                                     Id = p?.Id,
                                     Name = p?.Name,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     Status = v.Status,
                                     VersionNumber = v.VersionNumber
                                 };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        itemRecord = itemRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        itemRecord = itemRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                List<AllAutomationsViewModel> filterRecord = null;
                if (predicate != null)
                    filterRecord = itemRecord.ToList().FindAll(predicate);
                else
                    filterRecord = itemRecord.ToList();

                paginatedList.Items = filterRecord.Skip(skip).Take(take).ToList();

                paginatedList.Completed = itemsList.Completed;
                paginatedList.Impediments = itemsList.Impediments;
                paginatedList.PageNumber = itemsList.PageNumber;
                paginatedList.PageSize = itemsList.PageSize;
                paginatedList.ParentId = itemsList.ParentId;
                paginatedList.Started = itemsList.Started;
                paginatedList.TotalCount = filterRecord?.Count;
            }

            return paginatedList;
        }
    }
}
