using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenBots.Server.DataAccess.Repositories.File
{
    public class StorageFileRepository : EntityRepository<StorageFile>, IStorageFileRepository
    {
        public StorageFileRepository(StorageContext context, ILogger<StorageFile> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        { }

        protected override DbSet<StorageFile> DbTable()
        {
            return dbContext.StorageFiles;
        }

        public PaginatedList<FileFolderViewModel> FindAllView(Guid? driveId, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100, string path = null)
        {
            PaginatedList<FileFolderViewModel> paginatedList = new PaginatedList<FileFolderViewModel>();
            var itemsList = new PaginatedList<StorageFile>();
            if (string.IsNullOrEmpty(path))
                itemsList = base.Find(null, j => j.IsDeleted == false && j.StorageDriveId == driveId);
            else
            {
                path = path + Path.DirectorySeparatorChar;
                itemsList = base.Find(null, j => j.IsDeleted == false && j.StorageDriveId == driveId && j.StoragePath.Contains(path));
            }

            if (itemsList != null && itemsList.Items != null && itemsList.Items.Count > 0)
            {
                var itemRecord = from a in itemsList.Items
                                 join b in dbContext.StorageFolders on a.StorageFolderId equals b.Id into table1
                                 from b in table1.DefaultIfEmpty()
                                 join c in dbContext.StorageDrives on a.StorageDriveId equals c.Id into table2
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
                                     StorageDriveId = a?.StorageDriveId
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
