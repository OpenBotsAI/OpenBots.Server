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
    public class AssetRepository : EntityRepository<Asset>, IAssetRepository
    {
        public AssetRepository(StorageContext context, ILogger<Asset> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<Asset> DbTable()
        {
            return dbContext.Assets;
        }

        public PaginatedList<AssetViewModel> FindAllView(Predicate<AssetViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<AssetViewModel> paginatedList = new PaginatedList<AssetViewModel>();

            var itemsList = base.Find(null, j => j.IsDeleted == false);
            if (itemsList != null && itemsList.Items != null && itemsList.Items.Count > 0)
            {
                var itemRecord = from p in itemsList.Items
                                 join a in dbContext.Agents on p.AgentId equals a.Id into table1
                                 from a in table1.DefaultIfEmpty()
                                 join f in dbContext.StorageFiles on p.FileId equals f.Id into table2
                                 from f in table2.DefaultIfEmpty()
                                 select new AssetViewModel
                                 {
                                     Id = p?.Id,
                                     Name = p?.Name,
                                     Type = p?.Type,
                                     TextValue = p.TextValue,
                                     NumberValue = p.NumberValue,
                                     JsonValue = p.JsonValue,
                                     FileId = p?.FileId,
                                     SizeInBytes = p.SizeInBytes,
                                     AgentId = p.AgentId,
                                     CreatedOn = p.CreatedOn,
                                     CreatedBy = p.CreatedBy,
                                     AgentName = a?.Name,
                                     FileName = f?.Name
                                 };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        itemRecord = itemRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        itemRecord = itemRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                List<AssetViewModel> filterRecord = null;
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
