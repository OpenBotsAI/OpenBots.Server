using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel.Email;
using System;

namespace OpenBots.Server.DataAccess.Repositories.Interfaces
{
    public interface IEmailAttachmentRepository : IEntityRepository<EmailAttachment>
    {
        public PaginatedList<AllEmailAttachmentsViewModel> FindAllView(Guid emailId, Predicate<AllEmailAttachmentsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
    }
}
