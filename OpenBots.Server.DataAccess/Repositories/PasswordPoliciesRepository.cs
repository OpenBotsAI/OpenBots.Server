using OpenBots.Server.Model.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class PasswordPolicyRepository : EntityRepository<PasswordPolicy>, IPasswordPolicyRepository
    {
        public PasswordPolicyRepository(StorageContext context, 
            ILogger<PasswordPolicy> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
            
        }

        protected override DbSet<PasswordPolicy> DbTable()
        {
            return DbContext.PasswordPolicies;
        }
    }
}
