using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.File;

namespace OpenBots.Server.DataAccess.Repositories.File
{
    public class StorageDriveRepository : EntityRepository<StorageDrive>, IStorageDriveRepository
    {
        public StorageDriveRepository(StorageContext context, ILogger<StorageDrive> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<StorageDrive> DbTable()
        {
            return dbContext.StorageDrives;
        }
    }
}
