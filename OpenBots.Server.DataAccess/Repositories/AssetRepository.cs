using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class AssetRepository : EntityRepository<Asset>, IAssetRepository
    {
        public AssetRepository(StorageContext context, ILogger<Asset> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<Asset> DbTable()
        {
            return dbContext.Assets;
        }
    }
}
