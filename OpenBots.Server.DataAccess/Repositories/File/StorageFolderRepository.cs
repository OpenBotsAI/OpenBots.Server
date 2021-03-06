﻿using Microsoft.AspNetCore.Http;
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
    public class StorageFolderRepository : EntityRepository<StorageFolder>, IStorageFolderRepository
    {
        private readonly IStorageFileRepository _storageFileRepository;

        public StorageFolderRepository(StorageContext context, ILogger<StorageFolder> logger, IHttpContextAccessor httpContextAccessor, IStorageFileRepository storageFileRepository) : base(context, logger, httpContextAccessor)
        {
            this._storageFileRepository = storageFileRepository;
        }

        protected override DbSet<StorageFolder> DbTable()
        {
            return dbContext.StorageFolders;
        }

        public PaginatedList<FileFolderViewModel> FindAllView(Guid? driveId, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100, string path = null)
        {
            PaginatedList<FileFolderViewModel> paginatedList = new PaginatedList<FileFolderViewModel>();
            var itemsList = new PaginatedList<StorageFolder>();
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
                                 join b in dbContext.StorageFolders on a.ParentFolderId equals b.Id into table1
                                 from b in table1.DefaultIfEmpty()
                                 join c in dbContext.StorageFolders on a.Id equals c.ParentFolderId into table2
                                 from c in table1.DefaultIfEmpty()
                                 join d in dbContext.StorageDrives on a.StorageDriveId equals d.Id into table3
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

        public PaginatedList<FileFolderViewModel> FindAllFilesFoldersView(Guid? driveId, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100, string path = null)
        {
            PaginatedList<FileFolderViewModel> paginatedList = new PaginatedList<FileFolderViewModel>();
            var folderItemsList = new PaginatedList<StorageFolder>();
            if (string.IsNullOrEmpty(path))
                folderItemsList = base.Find(null, j => j.IsDeleted == false && j.StorageDriveId == driveId);
            else
            {
                path = path + Path.DirectorySeparatorChar;
                folderItemsList = base.Find(null, j => j.IsDeleted == false && j.StorageDriveId == driveId && j.StoragePath.Contains(path));
            }

            if (folderItemsList != null && folderItemsList.Items != null && folderItemsList.Items.Count > 0)
            {

                var folderItemRecord = from a in folderItemsList.Items
                                 join b in dbContext.StorageFolders on a.ParentFolderId equals b.Id into table1
                                 from b in table1.DefaultIfEmpty()
                                 join c in dbContext.StorageFolders on a.Id equals c.ParentFolderId into table2
                                 from c in table1.DefaultIfEmpty()
                                 join d in dbContext.StorageDrives on a.StorageDriveId equals d.Id into table3
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

                var fileItemsList = new PaginatedList<StorageFile>();
                if (string.IsNullOrEmpty(path))
                    fileItemsList = _storageFileRepository.Find(null, j => j.IsDeleted == false && j.StorageDriveId == driveId);
                else
                {
                    path = path + Path.DirectorySeparatorChar;
                    fileItemsList = _storageFileRepository.Find(null, j => j.IsDeleted == false && j.StorageDriveId == driveId && j.StoragePath.Contains(path));
                }

                if (fileItemsList != null && fileItemsList.Items != null && fileItemsList.Items.Count > 0)
                {
                    var fileItemRecord = from a in fileItemsList.Items
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
                else
                {
                    List<FileFolderViewModel> filterRecord = null;
                    if (predicate != null)
                        filterRecord = folderItemRecord.ToList().FindAll(predicate);
                    else
                        filterRecord = folderItemRecord.ToList();

                    paginatedList.Items = filterRecord.Skip(skip).Take(take).ToList();

                    paginatedList.Completed = folderItemsList.Completed;
                    paginatedList.Impediments = folderItemsList.Impediments;
                    paginatedList.PageNumber = folderItemsList.PageNumber;
                    paginatedList.PageSize = folderItemsList.PageSize;
                    paginatedList.ParentId = folderItemsList.ParentId;
                    paginatedList.Started = folderItemsList.Started;
                    paginatedList.TotalCount = filterRecord?.Count;
                }
            }

            return paginatedList;
        }
    }
}
