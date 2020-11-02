using OpenBots.Server.Model.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class LookupValueRepository : EntityRepository<LookupValue>, ILookupValueRepository
    {
        public LookupValueRepository(StorageContext context, ILogger<LookupValue> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<LookupValue> DbTable()
        {
            return DbContext.LookupValues;
        }
    }
}
