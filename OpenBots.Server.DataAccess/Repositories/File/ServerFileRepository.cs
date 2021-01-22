using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.File;

namespace OpenBots.Server.DataAccess.Repositories.File
{
    public class ServerFileRepository : EntityRepository<ServerFile>, IServerFileRepository
    {
        public ServerFileRepository(StorageContext context, ILogger<ServerFile> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<ServerFile> DbTable()
        {
            return dbContext.ServerFiles;
        }
    }
}
