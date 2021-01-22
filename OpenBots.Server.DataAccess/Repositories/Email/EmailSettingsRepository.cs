using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model.Configuration;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class EmailSettingsRepository : EntityRepository<EmailSettings>, IEmailSettingsRepository
    {
        public EmailSettingsRepository(StorageContext storageContext, ILogger<EmailSettings> logger, IHttpContextAccessor httpContextAccessor) : base(storageContext, logger, httpContextAccessor)
        { }

        protected override DbSet<EmailSettings> DbTable()
        {
            return DbContext.EmailSettings;
        }
    }
}
