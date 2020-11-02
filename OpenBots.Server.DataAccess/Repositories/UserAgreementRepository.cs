using OpenBots.Server.Model.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class UserAgreementRepository : ReadOnlyEntityRepository<UserAgreement>, IUserAgreementRepository
    {
        public UserAgreementRepository(StorageContext context, ILogger<UserAgreement> logger) : base(context,  logger)
        {
        }

        protected override DbSet<UserAgreement> DbTable()
        {
            return dbContext.UserAgreements;
        }
    }

}
