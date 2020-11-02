using OpenBots.Server.Model.Membership;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class OrganizationRepository : TenantEntityRepository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(StorageContext context, ILogger<Organization> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override Microsoft.EntityFrameworkCore.DbSet<Organization> DbTable()
        {
            return base.DbContext.Organizations;
        }
    }
}
