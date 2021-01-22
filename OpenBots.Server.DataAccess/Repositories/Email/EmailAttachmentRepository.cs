using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel.Email;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class EmailAttachmentRepository : EntityRepository<EmailAttachment>, IEmailAttachmentRepository
    {
        public EmailAttachmentRepository(StorageContext context, ILogger<EmailAttachment> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<EmailAttachment> DbTable()
        {
            return dbContext.EmailAttachments;
        }

        public PaginatedList<AllEmailAttachmentsViewModel> FindAllView(Guid emailId, Predicate<AllEmailAttachmentsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<AllEmailAttachmentsViewModel> paginatedList = new PaginatedList<AllEmailAttachmentsViewModel>();

            var itemsList = base.Find(null, j => j.IsDeleted == false && j.EmailId == emailId);
            List<Guid> binaryObjectIds = new List<Guid>();
            if (itemsList != null && itemsList.Items != null && itemsList.Items.Count > 0)
            {
                var itemRecord = from a in itemsList.Items
                                 join b in dbContext.BinaryObjects on a.BinaryObjectId equals b.Id into table1
                                 from b in table1.DefaultIfEmpty()
                                 select new AllEmailAttachmentsViewModel
                                 {
                                     BinaryObjectId = (Guid)(a?.BinaryObjectId),
                                     SizeInBytes = (long)(a?.SizeInBytes),
                                     EmailId = (Guid)(a?.EmailId),
                                     Name = b?.Name
                                 };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        itemRecord = itemRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        itemRecord = itemRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                List<AllEmailAttachmentsViewModel> filterRecord = null;
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
