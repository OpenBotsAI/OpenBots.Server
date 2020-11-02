using OpenBots.Server.Model.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{

    public class EmailVerificationRepository : EntityRepository<EmailVerification>, IEmailVerificationRepository
    {
        public EmailVerificationRepository(StorageContext context, ILogger<EmailVerification> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

    
        protected override DbSet<EmailVerification> DbTable()
        {
            return DbContext.EmailVerifications;
        }

        protected override Func<EmailVerification, bool> ParentFilter(Guid parentId)
        {
            return (o => o.PersonId == parentId);
        }
    }
}
