using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class ScheduleParameterRepository : EntityRepository<ScheduleParameter>, IScheduleParameterRepository
    {
        public ScheduleParameterRepository(StorageContext context, ILogger<ScheduleParameter> logger, IHttpContextAccessor httpContextAccessor) 
            : base(context, logger, httpContextAccessor)
        {
        }
        protected override DbSet<ScheduleParameter> DbTable()
        {
            return dbContext.ScheduleParameters;
        }
    }
}
