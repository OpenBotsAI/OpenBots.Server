using OpenBots.Server.Model.Core;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class ApplicationVersionRepository : EntityRepository<ApplicationVersion>, IApplicationVersionRepository
    {
        public ApplicationVersionRepository(StorageContext context, ILogger<ApplicationVersion> logger, IHttpContextAccessor httpContextAccessor) : base(context,  logger, httpContextAccessor)
        {
        }

        protected override Microsoft.EntityFrameworkCore.DbSet<ApplicationVersion> DbTable()
        {
            return DbContext.AppVersion;
        }
    }
}
