using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.File;

namespace OpenBots.Server.DataAccess.Repositories.File
{
    public class ServerFolderRepository : EntityRepository<ServerFolder>, IServerFolderRepository
    {
        public ServerFolderRepository(StorageContext context, ILogger<ServerFolder> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<ServerFolder> DbTable()
        {
            return dbContext.ServerFolders;
        }
    }
}
