using System;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;

namespace OpenBots.Server.DataAccess.Repositories
{
    public interface IReadOnlyEntityRepository<T> where T : class, IEntity, new()
    {
        void SetContext(UserSecurityContext context);
        int Count();
        int Count(Func<T, bool> predicate);
        bool Exists(Guid Id);

        PaginatedList<TViewModel> Find<TViewModel>(Guid? parentId = null, Func<T, bool> predicate = null, Func<T, object> sort = null, OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 0)
          where TViewModel : class, IViewModel<T, TViewModel>, new();

        PaginatedList<T> Find(int skip = 0, int take = 0);
        PaginatedList<T> Find(Guid? parentId = null, Func<T, bool> predicate = null, Func<T, object> sort = null, OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 0);
        T GetOne(Guid Id);
    }
}