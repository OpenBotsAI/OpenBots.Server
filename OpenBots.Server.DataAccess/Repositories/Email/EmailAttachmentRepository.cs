using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.Core;

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
    }
}
