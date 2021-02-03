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
    public class ServerFolderRepository : EntityRepository<ServerFolder>, IServerFolderRepository
    {
        private readonly IServerFileRepository serverFileRepository;

        public ServerFolderRepository(StorageContext context, ILogger<ServerFolder> logger, IHttpContextAccessor httpContextAccessor, IServerFileRepository serverFileRepository) : base(context, logger, httpContextAccessor)
        {
            this.serverFileRepository = serverFileRepository;
        }

        protected override DbSet<ServerFolder> DbTable()
        {
            return dbContext.ServerFolders;
        }

        public PaginatedList<FileFolderViewModel> FindAllView(Guid? driveId, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<FileFolderViewModel> paginatedList = new PaginatedList<FileFolderViewModel>();
            var itemsList = base.Find(null, j => j.IsDeleted == false && j.StorageDriveId == driveId);

            if (itemsList != null && itemsList.Items != null && itemsList.Items.Count > 0)
            {

                var itemRecord = from a in itemsList.Items
                                 join b in dbContext.ServerFolders on a.ParentFolderId equals b.Id into table1
                                 from b in table1.DefaultIfEmpty()
                                 join c in dbContext.ServerFolders on a.Id equals c.ParentFolderId into table2
                                 from c in table1.DefaultIfEmpty()
                                 join d in dbContext.ServerDrives on a.StorageDriveId equals d.Id into table3
                                 from d in table3.DefaultIfEmpty()
                                 select new FileFolderViewModel
                                 {
                                     Name = a?.Name,
                                     Id = a?.Id,
                                     ContentType = "Folder",
                                     CreatedBy = a?.CreatedBy,
                                     CreatedOn = a?.CreatedOn,
                                     FullStoragePath = a?.StoragePath,
                                     HasChild = c?.ParentFolderId != null ? true : false,
                                     IsFile = false,
                                     ParentId = a?.ParentFolderId,
                                     StoragePath = b?.StoragePath != null ? b?.StoragePath : d?.Name,
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

        public PaginatedList<FileFolderViewModel> FindAllFilesFoldersView(Guid? driveId, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<FileFolderViewModel> paginatedList = new PaginatedList<FileFolderViewModel>();
            var folderItemsList = base.Find(null, j => j.IsDeleted == false && j.StorageDriveId == driveId);

            if (folderItemsList != null && folderItemsList.Items != null && folderItemsList.Items.Count > 0)
            {

                var folderItemRecord = from a in folderItemsList.Items
                                 join b in dbContext.ServerFolders on a.ParentFolderId equals b.Id into table1
                                 from b in table1.DefaultIfEmpty()
                                 join c in dbContext.ServerFolders on a.Id equals c.ParentFolderId into table2
                                 from c in table1.DefaultIfEmpty()
                                 join d in dbContext.ServerDrives on a.StorageDriveId equals d.Id into table3
                                 from d in table3.DefaultIfEmpty()
                                 select new FileFolderViewModel
                                 {
                                     Name = a?.Name,
                                     Id = a?.Id,
                                     ContentType = "Folder",
                                     CreatedBy = a?.CreatedBy,
                                     CreatedOn = a?.CreatedOn,
                                     FullStoragePath = a?.StoragePath,
                                     HasChild = c?.ParentFolderId != null ? true : false,
                                     IsFile = false,
                                     ParentId = a?.ParentFolderId,
                                     StoragePath = b?.StoragePath != null ? b?.StoragePath : d?.Name,
                                     Size = a?.SizeInBytes,
                                     StorageDriveId = a?.StorageDriveId
                                 };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        folderItemRecord = folderItemRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        folderItemRecord = folderItemRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                var fileItemsList = serverFileRepository.Find(null, j => j.IsDeleted == false && j.ServerDriveId == driveId);

                if (fileItemsList != null && fileItemsList.Items != null && fileItemsList.Items.Count > 0)
                {
                    var fileItemRecord = from a in fileItemsList.Items
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
                            fileItemRecord = fileItemRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                        else if (direction == OrderByDirectionType.Descending)
                            fileItemRecord = fileItemRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                    List<FileFolderViewModel> filterRecord = null;
                    List<FileFolderViewModel> itemRecord = null;
                    if (folderItemRecord != null || folderItemRecord.Any())
                    {
                        itemRecord = folderItemRecord.ToList();
                        itemRecord.AddRange(fileItemRecord);
                    }
                    else
                        itemRecord = fileItemRecord.ToList();

                    if (predicate != null)
                        filterRecord = itemRecord.ToList().FindAll(predicate);
                    else
                        filterRecord = itemRecord.ToList();

                    paginatedList.Items = filterRecord.Skip(skip).Take(take).ToList();

                    paginatedList.Completed = fileItemsList.Completed;
                    paginatedList.Impediments = fileItemsList.Impediments;
                    paginatedList.PageNumber = fileItemsList.PageNumber;
                    paginatedList.PageSize = fileItemsList.PageSize;
                    paginatedList.ParentId = fileItemsList.ParentId;
                    paginatedList.Started = fileItemsList.Started;
                    paginatedList.TotalCount = filterRecord?.Count;
                }
            }

            return paginatedList;
        }
    }
}
