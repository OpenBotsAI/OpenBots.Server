using OpenBots.Server.Model.Membership;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class OrganizationMemberRepository : TenantEntityRepository<OrganizationMember>, IOrganizationMemberRepository
    {
        public OrganizationMemberRepository(StorageContext context, ILogger<OrganizationMember> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        public async IAsyncEnumerable<Guid> GetOrganizationIdsByPersonId(Guid personId)
        {
            List<OrganizationMember> memberships = base.GetMany(om => om.PersonId.Equals(personId)).ToListAsync().Result;
            foreach (OrganizationMember member in memberships)
            {
                yield return member.Id.Value;
            }
        }

        public OrganizationMember Approve(Guid personId, Guid organizationId, Guid approverPersonId)
        {
            OrganizationMember member = new OrganizationMember();
            member.PersonId = personId;
            member.OrganizationId = organizationId;
            member.ApprovedBy = approverPersonId.ToString();
            member.ApprovedOn = DateTime.UtcNow;
            return Add(member);
        }

        public OrganizationMember GetMember(Guid personId, Guid organizationId)
        {
            return GetMany(om => om.PersonId.Equals(personId) && om.OrganizationId.Equals(organizationId)).FirstOrDefault();
        }

        protected override Func<OrganizationMember, bool> ParentFilter(Guid parentId)
        {
            return (o => o.OrganizationId == parentId);
        }

        protected override DbSet<OrganizationMember> DbTable()
        {
            return DbContext.OrganizationMembers;
        }
    }
}
