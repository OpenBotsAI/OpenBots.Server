using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.File;

namespace OpenBots.Server.DataAccess.Repositories.File
{
    public class FileAttributeRepository : EntityRepository<FileAttribute>, IFileAttributeRepository
    {
        public FileAttributeRepository(StorageContext context, ILogger<FileAttribute> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<FileAttribute> DbTable()
        {
            return dbContext.FileAttributes;
        }
    }
}
