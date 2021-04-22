using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class AutomationParameterRepository : EntityRepository<AutomationParameter>, IAutomationParameterRepository
    {
        public AutomationParameterRepository(StorageContext context, ILogger<AutomationParameter> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<AutomationParameter> DbTable()
        {
            return dbContext.AutomationParameters;
        }
    }
}
