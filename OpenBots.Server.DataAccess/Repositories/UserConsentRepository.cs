using OpenBots.Server.Model.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class UserConsentRepository : EntityRepository<UserConsent>, IUserConsentRepository
    {
        public UserConsentRepository(StorageContext context,  ILogger<UserConsent> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<UserConsent> DbTable()
        {
            return dbContext.UserConsents;
        }
    }
}
