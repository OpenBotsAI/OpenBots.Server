using OpenBots.Server.Model.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class PersonEmailRepository : EntityRepository<PersonEmail>, IPersonEmailRepository
    {
        public PersonEmailRepository(StorageContext context, ILogger<PersonEmail> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<PersonEmail> DbTable()
        {
            return DbContext.PersonEmails;
        }

        protected override Func<PersonEmail, bool> ParentFilter(Guid parentId)
        {
            return (o => o.PersonId == parentId);
        }
    }
}
