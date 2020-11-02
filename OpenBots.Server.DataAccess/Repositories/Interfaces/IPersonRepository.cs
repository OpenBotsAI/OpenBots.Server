using OpenBots.Server.Model.Identity;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface IPersonRepository : IEntityRepository<Person>, ITenantEntityRepository<Person>
    {
    }
}
