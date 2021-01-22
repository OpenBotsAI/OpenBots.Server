using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class AccessRequestRepository : TenantEntityRepository<AccessRequest>, IAccessRequestRepository
    {
        public AccessRequestRepository(StorageContext context,  ILogger<AccessRequest> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        public AccessRequest GetPendingAccessRequest(Guid personId, Guid organizationId)
        {
            var arequests = base.GetMany(ar => ar.IsAccessRequested.Equals(true) && ar.PersonId.Equals(personId) && ar.OrganizationId.Equals(organizationId)).FirstOrDefaultAsync();
            return arequests.Result;
        }


        protected override Func<AccessRequest, bool> ParentFilter(Guid parentId)
        {
            return (o => o.OrganizationId == parentId);
        }

        protected override DbSet<AccessRequest> DbTable()
        {
            return DbContext.AccessRequests;
        }

        public override PaginatedList<AccessRequest> Find(Guid? parentId = null, Func<AccessRequest, bool> predicate = null, Func<AccessRequest, object> sort = null, OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 0)
        {
            var accessRequests = base.Find(parentId, predicate, sort, direction, skip, take);

            return accessRequests;
        }

        public override PaginatedList<AccessRequest> Find(int skip = 0, int take = 0)
        {
            return base.Find(skip, take);
        }

    }
}
