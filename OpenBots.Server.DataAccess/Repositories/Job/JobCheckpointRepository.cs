using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class JobCheckpointRepository : EntityRepository<JobCheckpoint>, IJobCheckpointRepository
    {
        public JobCheckpointRepository(StorageContext context, ILogger<JobCheckpoint> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<JobCheckpoint> DbTable()
        {
            return dbContext.JobCheckpoints;
        }
    }
}