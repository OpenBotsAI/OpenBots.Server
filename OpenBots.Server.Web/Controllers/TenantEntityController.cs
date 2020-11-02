using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers
{
    /// <summary>
    /// Controller for TenantEntity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TenantEntityController<T> : EntityController<T>
         where T : class, IEntity, ITenanted, new()
    {
        /// <summary>
        /// TenantEntityController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="membershipManager"></param>
        protected TenantEntityController(ITenantEntityRepository<T> repository,
          ApplicationIdentityUserManager userManager,
          IHttpContextAccessor httpContextAccessor,
          IMembershipManager membershipManager,
          IConfiguration configuration) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        { }
    }
}
