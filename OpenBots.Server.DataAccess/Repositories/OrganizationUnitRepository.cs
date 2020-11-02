using OpenBots.Server.Model.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class OrganizationUnitRepository : TenantEntityRepository<OrganizationUnit> , IOrganizationUnitRepository
    {
        public OrganizationUnitRepository(StorageContext context, ILogger<OrganizationUnit> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }
     
        protected override DbSet<OrganizationUnit> DbTable()
        {
            return DbContext.OrganizationUnits;
        }

        protected override Func<OrganizationUnit, bool> ParentFilter(Guid parentId)
        {
            return (o => o.OrganizationId.Equals(parentId));
        }
    }
}
