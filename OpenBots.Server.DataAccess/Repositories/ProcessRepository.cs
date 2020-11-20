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
    /// Process Repository
    /// </summary>
    public class ProcessRepository : EntityRepository<Process>, IProcessRepository
    {
        /// <summary>
        /// Construtor for Process Repository
        /// </summary>
        /// <param name="storageContext"></param>
        /// <param name="logger"></param>
        /// <param name="httpContextAccessor"></param>
        public ProcessRepository(StorageContext storageContext, ILogger<Process> logger, IHttpContextAccessor httpContextAccessor) :base(storageContext, logger, httpContextAccessor) 
        {
        }

        /// <summary>
        /// Retrieves processes
        /// </summary>
        /// <returns></returns>
        protected override DbSet<Process> DbTable()
        {
            return DbContext.Processes;
        }

        public PaginatedList<AllProcessesViewModel> FindAllView(Predicate<AllProcessesViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<AllProcessesViewModel> paginatedList = new PaginatedList<AllProcessesViewModel>();

            var itemsList = base.Find(null, j => j.IsDeleted == false);
            if (itemsList != null && itemsList.Items != null && itemsList.Items.Count > 0)
            {
                var itemRecord = from p in itemsList.Items
                                 join v in dbContext.ProcessVersions on p.Id equals v.ProcessId into table1
                                 from v in table1.DefaultIfEmpty()
                                 select new AllProcessesViewModel
                                 {
                                     Id = p?.Id,
                                     Name = p?.Name,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     Status = v.Status
                                 };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        itemRecord = itemRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        itemRecord = itemRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                List<AllProcessesViewModel> filterRecord = null;
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
