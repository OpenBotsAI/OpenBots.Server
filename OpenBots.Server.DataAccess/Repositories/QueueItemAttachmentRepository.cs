using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel.Queue;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class QueueItemAttachmentRepository : EntityRepository<QueueItemAttachment>, IQueueItemAttachmentRepository
    {
        public QueueItemAttachmentRepository(StorageContext context, ILogger<QueueItemAttachment> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<QueueItemAttachment> DbTable()
        {
            return dbContext.QueueItemAttachments;
        }

        public PaginatedList<AllQueueItemAttachmentsViewModel> FindAllView(Guid queueItemId, Predicate<AllQueueItemAttachmentsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<AllQueueItemAttachmentsViewModel> paginatedList = new PaginatedList<AllQueueItemAttachmentsViewModel>();

            var itemsList = base.Find(null, j => j.IsDeleted == false && j.QueueItemId == queueItemId);
            List<Guid> binaryObjectIds = new List<Guid>();
            if (itemsList != null && itemsList.Items != null && itemsList.Items.Count > 0)
            {
                var itemRecord = from a in itemsList.Items
                                 join b in dbContext.BinaryObjects on a.BinaryObjectId equals b.Id into table1
                                 from b in table1.DefaultIfEmpty()
                                 select new AllQueueItemAttachmentsViewModel
                                 {
                                     BinaryObjectId = (Guid)(a?.BinaryObjectId),
                                     SizeInBytes = (long)(a?.SizeInBytes),
                                     QueueItemId = (Guid)(a?.QueueItemId),
                                     Name = b?.Name
                                 };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        itemRecord = itemRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        itemRecord = itemRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                List<AllQueueItemAttachmentsViewModel> filterRecord = null;
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
