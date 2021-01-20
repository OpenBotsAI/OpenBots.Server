using System;
using System.Collections.Generic;
using OpenBots.Server.Model.Membership;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface IOrganizationMemberRepository 
            : IEntityRepository<OrganizationMember>, ITenantEntityRepository<OrganizationMember>
    {
        OrganizationMember Approve(Guid personId, Guid organizationId, Guid approverPersonId);
        OrganizationMember GetMember(Guid personId, Guid organizationId);
        IAsyncEnumerable<Guid> GetOrganizationIdsByPersonId(Guid personId);
    }
}