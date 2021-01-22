using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class IPFencingRepository : EntityRepository<IPFencing>, IIPFencingRepository
    {
        public IPFencingRepository(StorageContext context, ILogger<IPFencing> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<IPFencing> DbTable()
        {
            return dbContext.IPFencings;
        }

        protected override Func<IPFencing, bool> ParentFilter(Guid parentId)
        {
            return (o => o.OrganizationId == parentId);
        }
        public override PaginatedList<IPFencing> Find(Guid? parentId = null, Func<IPFencing, bool> predicate = null, Func<IPFencing, object> sort = null, OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 0)
        {
            var iPFencingRules = base.Find(parentId, predicate, sort, direction, skip, take);

            return iPFencingRules;
        }
        public override PaginatedList<IPFencing> Find(int skip = 0, int take = 0)
        {
            return base.Find(skip, take);
        }
    }
}
