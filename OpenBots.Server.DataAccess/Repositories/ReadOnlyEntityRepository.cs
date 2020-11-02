using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace OpenBots.Server.DataAccess.Repositories
{
    public abstract class ReadOnlyEntityRepository<T> : IReadOnlyEntityRepository<T> where T : class, IEntity, new()
    {
        /// <summary>
        /// The database context/
        /// </summary>
        protected StorageContext dbContext;

        /// <summary>
        /// The user context
        /// </summary>
        private UserSecurityContext userContext;
        private readonly ILogger<T> logger;

        protected UserSecurityContext UserContext { get => userContext; set => userContext = value; }

        protected StorageContext DbContext => dbContext;

        public void SetContext(UserSecurityContext context)
        {
            UserContext = context;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRepository{T}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        public ReadOnlyEntityRepository(StorageContext context, ILogger<T> logger)
        {
            dbContext = context;
            this.logger = logger;
        }

        /// <summary>
        /// Requesting Authorization the entire record set before serving it.
        /// Override if you need entire Resultset for validation and a Lambda is not good enough.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns></returns>
        protected virtual IQueryable<T> AuthorizeRows(IQueryable<T> entities)
        {
            return entities.Where(t => AuthorizeRow(t));
        }

        protected virtual bool AuthorizeOrg(Guid? organizationId)
        {
            return true;
        }

        /// <summary>
        /// Authorizes the row.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected virtual bool AuthorizeRow(T entity)
        {
            return true;
        }

        /// <summary>
        /// Authorizes the read.
        /// </summary>
        /// <returns></returns>
        protected virtual Func<T, bool> AuthorizeRead()
        {
            return (o => true);
        }

        protected abstract DbSet<T> DbTable();

        /// <summary>
        /// Finds the specified skip.
        /// </summary>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        public virtual PaginatedList<T> Find(int skip = 0, int take = 0)
        {
            int pageNumber = skip;
            if (skip != 0 && take != 0)
                pageNumber = skip / take;

            PaginatedList<T> list = new PaginatedList<T>
            {
                TotalCount = Count(),
                PageNumber = pageNumber, // skip / take,
                PageSize = take
            };
            if (skip == 0 || take == 0)
                list.Items = GetMany().ToList();
            else
                list.Items = GetMany().Skip(skip).Take(take).ToList();

            return list;
        }

        /// <summary>
        /// Finds the specified predicate.
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="sort"></param>
        /// <param name="direction"></param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        public virtual PaginatedList<T> Find(Guid? parentId = null, Func<T, bool> predicate = null, Func<T, object> sort = null, OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 0)
        {
            PaginatedList<T> list = new PaginatedList<T>();

            if (skip == 0 || take == 0)
            {
                list.PageNumber = 0;
                list.PageSize = 0;
            }
            else
            {
                int pageNumber = skip;
                if (skip != 0 && take != 0)
                    pageNumber = skip / take;

                list.PageNumber = pageNumber;
                list.PageSize = take;
            }

            Tuple<IQueryable<T>, int> result = Query(parentId, predicate, sort, direction, skip, take);

            list.TotalCount = result.Item2;
            list.Items = result.Item1.ToList();

            return list;
        }

        /// <summary>
        /// Finds the specified predicate.
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="sort"></param>
        /// <param name="direction"></param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        public virtual PaginatedList<TViewModel> Find<TViewModel>(Guid? parentId = null, Func<T, bool> predicate = null, Func<T, object> sort = null, OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 0)
            where TViewModel  : class, IViewModel<T, TViewModel>, new()
        {
            PaginatedList<TViewModel> list = new PaginatedList<TViewModel>();

            if (skip == 0 || take == 0)
            {
                list.PageNumber = 0;
                list.PageSize = 0;
            }
            else
            {
                int pageNumber = skip;
                if (skip != 0 && take != 0)
                    pageNumber = skip / take;

                list.PageNumber = pageNumber;
                list.PageSize = take;
            }

            Tuple<IQueryable<T>, int> result = Query(parentId, predicate, sort, direction, skip, take);

            list.TotalCount = result.Item2;
            TViewModel transform = new TViewModel();
            list.Items = result.Item1.Select(m => transform.Map(m)).ToList();

            return list;
        }

        /// <summary>
        /// Finds the specified predicate.
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="sort"></param>
        /// <param name="direction"></param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns></returns>
        protected virtual Tuple<IQueryable<T>, int> Query(Guid? parentId = null, Func<T, bool> predicate = null, Func<T, object> sort = null, OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 0)
        {
            IQueryable<T> resultSet;
            if (predicate != null)
                resultSet = GetMany(predicate, parentId);
            else
                resultSet = GetMany(null, parentId);

            int totalCount = resultSet.Count();

            if (sort != null)
            {
                if (direction == OrderByDirectionType.Ascending)
                    resultSet = resultSet.OrderBy(sort).AsQueryable();
                else
                    resultSet = resultSet.OrderByDescending(sort).AsQueryable();
            }

            if (skip != 0)
                resultSet = resultSet.Skip(skip);

            if (take != 0)
                resultSet = resultSet.Take(take);

            return new Tuple<IQueryable<T>, int>(resultSet, totalCount);
        }

        protected IQueryable<T> GetMany(Func<T, bool> predicate = null, Guid? parentId = null)
        {
            IQueryable<T> resultSet = DbTable().AsQueryable();

            if (parentId.HasValue && parentId.Value != Guid.Empty)
                resultSet = resultSet.Where(ParentFilter(parentId.Value)).AsQueryable();

            resultSet = resultSet.Where(RemoveSoftDeleted()).AsQueryable();
            resultSet = AuthorizeRows(resultSet.Where(AuthorizeRead()).AsQueryable());

            if (predicate != null)
                resultSet = resultSet.Where(predicate).AsQueryable();

            return resultSet;
        }

        protected virtual Func<T, bool> ParentFilter(Guid parentId)
        {
            return (o => true);
        }

        /// <summary>
        /// Counts the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public int Count(Func<T, bool> predicate)
        {
            return DbTable().Where(AuthorizeRead()).AsQueryable().Where(predicate).Count();
        }

        /// <summary>
        /// Counts this instance.
        /// </summary>
        /// <returns></returns>
        public virtual int Count()
        {
            return Count(en => true);
        }

        /// <summary>
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public T GetOne(Guid Id)
        {
            var result = DbTable().Where(AuthorizeRead()).AsQueryable().Where(e => e.Id.Equals(Id)).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Existses the specified identifier.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <returns></returns>
        public bool Exists(Guid Id)
        {
            return DbTable().Where(AuthorizeRead()).AsQueryable().Where(e => e.Id == Id).Any();
        }

        protected virtual Func<T, bool> RemoveSoftDeleted()
        {
            return (o => o.IsDeleted == false);
        }
    }
}
