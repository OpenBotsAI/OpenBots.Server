using OpenBots.Server.Model.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public abstract class TenantEntityRepository<T> 
        : EntityRepository<T>, ITenantEntityRepository<T> 
        where T : class, IEntity, ITenanted, new()
    {
        bool ignoreSecurity = false;

        public void ForceIgnoreSecurity()
        {
            ignoreSecurity = true;
        }

        public void ForceSecurity()
        {
            ignoreSecurity = false;
        }

        public TenantEntityRepository(StorageContext context, ILogger<T> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {

        }

        /// <summary>
        /// Authorizes the row.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected override bool AuthorizeRow(T entity)
        {
            // If Security Has been Forcefully Switched Off by Calling ForceIgnoreSecurity() method
            if (ignoreSecurity)
                return true;

            // If the Entity is NULL or OrganizationID is empty, there is nothing to check.
            // However we need to make sure that End users do not have option to Add Records that have OrganizationID as NULL/Empty.
            if (entity == null|| !entity.OrganizationId.HasValue || entity.OrganizationId.Value.Equals(Guid.Empty))
                return true;

            if (UserContext != null)
            {
                if (UserContext.OrganizationId != null && UserContext.OrganizationId.Any())
                {
                    return UserContext.OrganizationId.Contains(entity.OrganizationId.Value);
                }
                return false;
            }
            else
                return false;
        }

        protected override bool AuthorizeOrg(Guid? organizationId)
        {
            if (ignoreSecurity)
                return true;

            // If the Entity is NULL or OrganizationID is empty, there is nothing to check.
            // However we need to make sure that End users do not have option to Add Records that have OrganizationID as NULL/Empty.
            if (organizationId == null)
                return true;

            if (UserContext != null)
            {
                if (UserContext.OrganizationId != null && UserContext.OrganizationId.Any())
                {
                    return UserContext.OrganizationId.Contains(organizationId.Value);
                }
                return false;
            }
            else
                return false;
        }

        /// <summary>
        /// Authorizes the read.
        /// </summary>
        /// <returns></returns>
        protected override Func<T, bool> AuthorizeRead()
        {
            return (o => AuthorizeRow(o));
        }

        /// <summary>
        /// Authorizes the operation.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        protected override bool AuthorizeOperation(T entity, EntityOperationType operation)
        {
            // get value of AuthorizeRow and if false then no acccess
            return AuthorizeRow(entity);
        }
    }
}
