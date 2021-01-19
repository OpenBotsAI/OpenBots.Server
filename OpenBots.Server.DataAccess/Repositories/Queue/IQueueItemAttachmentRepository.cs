using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel.Queue;
using System;

namespace OpenBots.Server.DataAccess.Repositories.Interfaces
{
    public interface IQueueItemAttachmentRepository : IEntityRepository<QueueItemAttachment>
    {

        PaginatedList<AllQueueItemAttachmentsViewModel> FindAllView(Guid queueItemId, Predicate<AllQueueItemAttachmentsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
    }
}
