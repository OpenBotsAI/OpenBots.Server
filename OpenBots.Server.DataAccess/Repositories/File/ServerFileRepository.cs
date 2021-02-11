using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.DataAccess.Repositories.File
{
    public class ServerFileRepository : EntityRepository<ServerFile>, IServerFileRepository
    {
        public ServerFileRepository(StorageContext context, ILogger<ServerFile> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        { }

        protected override DbSet<ServerFile> DbTable()
        {
            return dbContext.ServerFiles;
        }

        public PaginatedList<FileFolderViewModel> FindAllView(Guid? driveId, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<FileFolderViewModel> paginatedList = new PaginatedList<FileFolderViewModel>();
            var itemsList = base.Find(null, j => j.IsDeleted == false && j.ServerDriveId == driveId);

            if (itemsList != null && itemsList.Items != null && itemsList.Items.Count > 0)
            {
                var itemRecord = from a in itemsList.Items
                                 join b in dbContext.ServerFolders on a.StorageFolderId equals b.Id into table1
                                 from b in table1.DefaultIfEmpty()
                                 join c in dbContext.ServerDrives on a.ServerDriveId equals c.Id into table2
                                 from c in table2.DefaultIfEmpty()
                                 select new FileFolderViewModel
                                 {
                                     Name = a?.Name,
                                     Id = a?.Id,
                                     ContentType = a?.ContentType,
                                     CreatedBy = a?.CreatedBy,
                                     CreatedOn = a?.CreatedOn,
                                     FullStoragePath = a?.StoragePath,
                                     HasChild = false,
                                     IsFile = true,
                                     ParentId = a?.StorageFolderId,
                                     StoragePath = b?.StoragePath != null ? b?.StoragePath : c?.Name,
                                     Size = a?.SizeInBytes,
                                     StorageDriveId = a?.ServerDriveId
                                 };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        itemRecord = itemRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        itemRecord = itemRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                List<FileFolderViewModel> filterRecord = null;
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
