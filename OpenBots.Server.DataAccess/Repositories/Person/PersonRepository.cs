using OpenBots.Server.Model.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class PersonRepository : TenantEntityRepository<Person>, IPersonRepository
    {
        public PersonRepository(StorageContext context,  ILogger<Person> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<Person> DbTable()
        {
            return DbContext.People;
        }
    }
}
