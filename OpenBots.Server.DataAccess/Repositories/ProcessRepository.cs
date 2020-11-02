using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;

namespace OpenBots.Server.DataAccess.Repositories
{
    /// <summary>
    /// Process Repository
    /// </summary>
    public class ProcessRepository : EntityRepository<Process>, IProcessRepository
    {
        /// <summary>
        /// Construtor for Process Repository
        /// </summary>
        /// <param name="storageContext"></param>
        /// <param name="logger"></param>
        /// <param name="entityEventSink"></param>
        public ProcessRepository(StorageContext storageContext, ILogger<Process> logger, IHttpContextAccessor httpContextAccessor) :base(storageContext, logger, httpContextAccessor) 
        {
        }

        /// <summary>
        /// Retrieves processes
        /// </summary>
        /// <returns></returns>
        protected override DbSet<Process> DbTable()
        {
            return DbContext.Processes;
        }
    }
}
