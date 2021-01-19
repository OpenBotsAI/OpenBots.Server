using OpenBots.Server.Model.Core;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface ITenantEntityRepository<T> : IEntityRepository<T>
          where T : class, IEntity, ITenanted, new()
    {
        void ForceIgnoreSecurity();
        void ForceSecurity();
    }
}