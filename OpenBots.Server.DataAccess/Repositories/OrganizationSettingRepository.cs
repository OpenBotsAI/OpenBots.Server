using OpenBots.Server.Model.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{

    public class OrganizationSettingRepository : TenantEntityRepository<OrganizationSetting>, IOrganizationSettingRepository
    {
        public OrganizationSettingRepository(StorageContext context,  ILogger<OrganizationSetting> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<OrganizationSetting> DbTable()
        {
            return DbContext.OrganizationSettings;
        }

        protected override Func<OrganizationSetting, bool> ParentFilter(Guid parentId)
        {
            return (o => o.OrganizationId == parentId);
        }
    }
}
