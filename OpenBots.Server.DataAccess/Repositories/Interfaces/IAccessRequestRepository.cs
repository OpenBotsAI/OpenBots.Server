using System;
using OpenBots.Server.Model.Membership;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface IAccessRequestRepository : ITenantEntityRepository<AccessRequest>
    {
        AccessRequest GetPendingAccessRequest(Guid personId, Guid organizationId);
    }
}