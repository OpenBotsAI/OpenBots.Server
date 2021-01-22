using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.Configuration;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class ConfigurationValueRepository : EntityRepository<ConfigurationValue>, IConfigurationValueRepository
    {
        public ConfigurationValueRepository(StorageContext context, ILogger<ConfigurationValue> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<ConfigurationValue> DbTable()
        {
            return dbContext.ConfigurationValues;
        }
    }
}
