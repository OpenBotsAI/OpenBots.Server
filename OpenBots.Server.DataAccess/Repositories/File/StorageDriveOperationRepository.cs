using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.File;

namespace OpenBots.Server.DataAccess.Repositories.File
{
    public class StorageDriveOperationRepository : EntityRepository<StorageDriveOperation>, IStorageDriveOperationRepository
    {
        public StorageDriveOperationRepository(StorageContext context, ILogger<StorageDriveOperation> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<StorageDriveOperation> DbTable()
        {
            return dbContext.StorageDriveOperations;
        }
    }
}
