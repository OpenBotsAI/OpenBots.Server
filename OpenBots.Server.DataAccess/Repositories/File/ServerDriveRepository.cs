using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.File;

namespace OpenBots.Server.DataAccess.Repositories.File
{
    public class ServerDriveRepository : EntityRepository<ServerDrive>, IServerDriveRepository
    {
        public ServerDriveRepository(StorageContext context, ILogger<ServerDrive> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<ServerDrive> DbTable()
        {
            return dbContext.ServerDrives;
        }
    }
}
