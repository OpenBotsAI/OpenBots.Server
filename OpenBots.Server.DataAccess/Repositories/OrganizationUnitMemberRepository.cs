using OpenBots.Server.Model.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class OrganizationUnitMemberRepository : TenantEntityRepository<OrganizationUnitMember>, IOrganizationUnitMemberRepository
    {
        public OrganizationUnitMemberRepository(StorageContext context,  ILogger<OrganizationUnitMember> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<OrganizationUnitMember> DbTable()
        {
            return DbContext.OrganizationUnitMembers;
        }

        protected override Func<OrganizationUnitMember, bool> ParentFilter(Guid parentId)
        {
            return (o => o.OrganizationId == parentId);
        }
    }
}
