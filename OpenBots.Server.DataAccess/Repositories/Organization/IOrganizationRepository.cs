using OpenBots.Server.Model.Membership;
using System;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface IOrganizationRepository : IEntityRepository<Organization>, ITenantEntityRepository<Organization>
    {
    }
}
