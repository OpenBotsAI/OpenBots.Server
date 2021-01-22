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
    public class QueueItemRepository : EntityRepository<QueueItem>, IQueueItemRepository
    {
        private readonly StorageContext context;
        public QueueItemRepository(StorageContext context, ILogger<QueueItem> logger,IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
            this.context = context;
        }

        protected override DbSet<QueueItem> DbTable()
        {
            return dbContext.QueueItems;
        }

        public PaginatedList<AllQueueItemsViewModel> FindAllView(Predicate<AllQueueItemsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<AllQueueItemsViewModel> paginatedList = new PaginatedList<AllQueueItemsViewModel>();

            var itemsList = base.Find(null, j => j.IsDeleted == false);
            List<Guid> binaryObjectIds = new List<Guid>();
            if (itemsList != null && itemsList.Items != null && itemsList.Items.Count > 0)
            {
                var itemRecord = from q in itemsList.Items
                                 join a in dbContext.QueueItemAttachments on q.Id equals a.QueueItemId into table1
                                 select new AllQueueItemsViewModel
                                 {
                                     Id = q?.Id,
                                     Name = q?.Name,
                                     State = q?.State,
                                     StateMessage = q?.StateMessage,
                                     IsLocked = q.IsLocked,
                                     LockedBy = q?.LockedBy,
                                     LockedOnUTC = q?.LockedOnUTC,
                                     LockedUntilUTC = q?.LockedUntilUTC,
                                     LockedEndTimeUTC = q?.LockedEndTimeUTC,
                                     ExpireOnUTC = q?.ExpireOnUTC,
                                     PostponeUntilUTC = q?.PostponeUntilUTC,
                                     ErrorCode = q?.ErrorCode,
                                     ErrorMessage = q?.ErrorMessage,
                                     ErrorSerialized = q?.ErrorSerialized,
                                     Source = q?.Source,
                                     Event = q?.Event,
                                     ResultJSON = q?.ResultJSON,
                                     QueueId = q?.QueueId,
                                     CreatedOn = q?.CreatedOn,
                                     PayloadSizeInBytes = q?.PayloadSizeInBytes,
                                     //list of all binary object ids that correlate to the queue item
                                     BinaryObjectIds = context.QueueItemAttachments.Where(a => a.QueueItemId == q.Id)?.Select(a  => a.BinaryObjectId)?.ToList()
                                 };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        itemRecord = itemRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        itemRecord = itemRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                List<AllQueueItemsViewModel> filterRecord = null;
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
