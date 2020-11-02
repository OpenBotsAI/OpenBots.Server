using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.Model.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using OpenBots.Server.Model;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    /// <summary>
    /// Entity Repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EntityRepository<T> : ReadOnlyEntityRepository<T>,
        IEntityRepository<T> where T : class, IEntity, new()
    {
        const string DUPLICATE_KEY_ERROR_MESSSAGE = @"Cannot insert duplicate key row in object '(.*)' with unique index '(.*)'. The duplicate key value is (.*).The statement has been terminated.";

        private readonly ClaimsPrincipal _caller;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityRepository{T}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpContextAccessor"></param>
        public EntityRepository(StorageContext context, ILogger<T> logger, IHttpContextAccessor httpContextAccessor)
            : base(context, logger)
        {
            _caller = (httpContextAccessor.HttpContext != null) ? httpContextAccessor.HttpContext.User : new ClaimsPrincipal(); 
        }

        /// <summary>
        /// Authorizes the operation.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        protected virtual bool AuthorizeOperation(T entity, EntityOperationType operation)
        {
            return true;
        }

        /// <summary>
        /// Add entity to appropriate data table
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="EntityValidationException"></exception>
        /// <exception cref="EntityAlreadyExistsException"></exception>
        /// <exception cref="EntityOperationException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        public virtual T Add(T entity)
        {
            if (entity == null)
                throw new EntityOperationException(new ArgumentNullException(nameof(entity)));

            if (AuthorizeOperation(entity, EntityOperationType.Add))
            {
                ValidationResults validation = Validate(entity);
                if (!validation.IsValid)
                    throw new EntityValidationException(validation);

                if (Exists(entity.Id.Value))
                    throw new EntityAlreadyExistsException();

                //get null value from database because it hasn't been added yet
                PropertyValues originalValues = null;
                var currentValues = DbContext.Entry(entity).CurrentValues;
                var newValues = currentValues.Clone();

                var savedEntity = DbContext.Add(entity);

                T newEntity = new T();
                ChangeNonAuditableProperties(newValues, entity, newEntity);

                newValues = DbContext.Entry(newEntity).CurrentValues;

                try
                {
                    TrackChange(entity, EntityOperationType.Add, newValues, originalValues);
                    DbContext.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.GetType() == typeof(Microsoft.Data.SqlClient.SqlException))
                    {
                        HandleDuplicateConstraint(ex.InnerException as Microsoft.Data.SqlClient.SqlException);
                    }
                    throw new EntityOperationException(ex);

                }
                catch (Exception ex)
                {
                    throw new EntityOperationException(ex);
                }

                return (T)savedEntity.Entity;
            }
            throw new UnauthorizedOperationException(EntityOperationType.Add);
        }

        /// <summary>
        /// Duplicate constraint error handling
        /// </summary>
        /// <param name="ex"></param>
        /// <exception cref="CannotInsertDuplicateConstraintException"></exception>
        private static void HandleDuplicateConstraint(Microsoft.Data.SqlClient.SqlException ex)
        {

            Regex rx = new Regex(DUPLICATE_KEY_ERROR_MESSSAGE,
                RegexOptions.CultureInvariant |
                RegexOptions.IgnoreCase);
            Match match = rx.Match(ex.Message.ToUpperInvariant().Replace("\r", "", StringComparison.InvariantCultureIgnoreCase).Replace("\n", "", StringComparison.InvariantCultureIgnoreCase));
            if (match.Success && match.Groups.Count == 4)
            {
                string tableName = match.Groups[1].Value;

                if (!string.IsNullOrEmpty(tableName) && tableName.Contains(".", StringComparison.InvariantCultureIgnoreCase))
                    tableName = tableName.Split('.').LastOrDefault();

                string constraintName = match.Groups[2].Value;
                if (!string.IsNullOrEmpty(constraintName) && constraintName.Contains("_", StringComparison.InvariantCultureIgnoreCase))
                    constraintName = constraintName.Split('_').LastOrDefault();

                string valueName = match.Groups[3].Value;

                throw new CannotInsertDuplicateConstraintException(ex, tableName, constraintName, valueName);

            }

            throw new CannotInsertDuplicateConstraintException(ex, "", "", "");
        }

        /// <summary>Deletes the specified identifier.</summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="EntityDoesNotExistException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        /// <exception cref="EntityOperationException"></exception>
        public virtual void Delete(Guid id)
        {
            //TODO: test when a controller uses this function (no controller has this functionality yet)
            if (!Exists(id))
                throw new EntityDoesNotExistException();

            T entity = GetOne(id);

            if (!AuthorizeOperation(entity, EntityOperationType.Delete))
            {
                throw new UnauthorizedOperationException(EntityOperationType.Add);
            }

            var originalValues = DbContext.Entry(entity).GetDatabaseValues();
            var oldValues = originalValues.Clone();

            T oldEntity = new T();
            ChangeNonAuditableProperties(oldValues, entity, oldEntity);

            var currentValues = DbContext.Entry(entity).CurrentValues;
            DbContext.Remove(entity);

            oldValues = DbContext.Entry(oldEntity).CurrentValues;

            try
            {
                TrackChange(entity, EntityOperationType.HardDelete, currentValues, oldValues);
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new EntityOperationException(ex);
            }
        }

        /// <summary>
        /// Softs the delete.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="EntityDoesNotExistException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        /// <exception cref="EntityOperationException"></exception>
        public virtual void SoftDelete(Guid id)
        {
            if (!Exists(id))
                throw new EntityDoesNotExistException();

            T entity = GetOne(id);

            if (!AuthorizeOperation(entity, EntityOperationType.Delete))
            {
                throw new UnauthorizedOperationException(EntityOperationType.Add);
            }

            var originalValues = DbContext.Entry(entity).GetDatabaseValues();
            var oldValues = originalValues.Clone();

            T oldEntity = new T();
            ChangeNonAuditableProperties(oldValues, entity, oldEntity);
            oldValues = DbContext.Entry(oldEntity).CurrentValues;

            entity.IsDeleted = true;
            entity.DeleteOn = DateTime.UtcNow;
            entity.DeletedBy = _caller.Identity.Name;
            DbContext.Update(entity);

            var currentValues = DbContext.Entry(entity).CurrentValues;
            var newValues = currentValues;

            T newEntity = new T();
            ChangeNonAuditableProperties(newValues, entity, newEntity);
            newValues = DbContext.Entry(newEntity).CurrentValues;

            try
            {
                TrackChange(entity, EntityOperationType.Delete, newValues, oldValues);
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new EntityOperationException(ex);
            }
        }

        /// <summary>
        /// Updates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="originalTimestamp">Timestamp value of original </param>
        /// <returns></returns>
        /// <exception cref="EntityValidationException"></exception>
        /// <exception cref="EntityDoesNotExistException"></exception>
        /// <exception cref="EntityConcurrencyException"></exception>
        /// <exception cref="EntityOperationException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        public virtual T Update(T entity, byte[] originalTimestamp = null)
        {
            if (entity == null)
                throw new EntityOperationException(new ArgumentNullException(nameof(entity)));

            if (AuthorizeOperation(entity, EntityOperationType.Update))
            {
                ValidationResults validation = Validate(entity);
                if (!validation.IsValid)
                    throw new EntityValidationException(validation);

                if (!Exists(entity.Id.Value))
                    throw new EntityDoesNotExistException();

                try
                {
                    var originalValues = DbContext.Entry(entity).OriginalValues;
                    var oldValues = originalValues.Clone();

                    T oldEntity = new T();
                    ChangeNonAuditableProperties(oldValues, entity, oldEntity);
                    oldValues = DbContext.Entry(oldEntity).CurrentValues;

                    if (originalTimestamp != null)
                        entity.Timestamp = originalTimestamp;

                    entity.UpdatedOn = DateTime.UtcNow;
                    entity.UpdatedBy = _caller.Identity.Name;

                    DbContext.Entry(entity).State = EntityState.Modified;
                    DbContext.SaveChanges();

                    var currentValues = DbContext.Entry(entity).CurrentValues;
                    var newValues = currentValues.Clone();

                    T newEntity = new T();
                    ChangeNonAuditableProperties(newValues, entity, newEntity);
                    newValues = DbContext.Entry(newEntity).CurrentValues;

                    TrackChange(entity, EntityOperationType.Update, newValues, oldValues);
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.GetType() == typeof(Microsoft.Data.SqlClient.SqlException))
                    {
                        HandleDuplicateConstraint(ex.InnerException as Microsoft.Data.SqlClient.SqlException);
                    }
                    throw new EntityOperationException(ex);

                }
                catch (Exception ex)
                {
                    throw new EntityOperationException(ex);
                }

                return entity;
            }
            throw new UnauthorizedOperationException(EntityOperationType.Add);
        }

     
        /// <summary>
        /// This method creates a changeset style model for Changes being made to the entity.
        /// This can be used to create audit logs or event queues in conjunction with IEntityOperationEventSink
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="entity"></param>
        /// <param name="operation"></param>
        /// <param name="currentValues"></param>
        /// <param name="originalValues"></param>
        /// <returns></returns>

        protected string TrackChange(T entity, EntityOperationType operation = EntityOperationType.Unknown, PropertyValues currentValues = null, PropertyValues originalValues = null, byte[] timestamp = null)
        {
            bool nonAuditable = typeof(INonAuditable).IsAssignableFrom(typeof(T));
            if (nonAuditable.Equals(false))
            {
                //create new AuditLog object
                AuditLog change = new AuditLog();

                try
                {
                    //define properties in object
                    change.Id = Guid.NewGuid();
                    change.ObjectId = entity.Id;

                    change.CreatedBy = _caller.Identity == null ? entity.CreatedBy : _caller.Identity.Name;
                    change.CreatedOn = DateTime.UtcNow;

                    if (timestamp == null)
                        timestamp = new byte[1];

                    change.Timestamp = timestamp;
                    //name of service that is being changed
                    change.ServiceName = entity.ToString();
                    //name of how entity is being changed (Add, Update, Delete)
                    change.MethodName = operation.ToString();
                    change.ParametersJson = ""; //TODO: update to show parameters of method
                    change.ExceptionJson = ""; //TODO: update to show any exceptions that arise
                    change.ChangedFromJson = (originalValues == null) ? null : JsonConvert.SerializeObject(originalValues.ToObject());
                    change.ChangedToJson = (currentValues == null) ? null : JsonConvert.SerializeObject(currentValues.ToObject());

                    //add object to AuditLog data table
                    DbContext.Add(change);
                    DbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    throw new EntityOperationException(ex);
                }

                //return audit log object as a string
                return change.ToString();
            }

            return new InvalidOperationException().Message;
        }

        private static void ChangeNonAuditableProperties(PropertyValues propertyValues, T entity, T newEntity)
        {
            foreach (var property in propertyValues.Properties)
            {
                string propertyName = property.Name;
                SetPropertyValue(propertyName, entity, newEntity);
            }
        }

        private static bool SetPropertyValue(string propertyName, T entity, T newEntity)
        {
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName);
            DoNotAuditAttribute doNotAudit = (DoNotAuditAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(DoNotAuditAttribute));

            if (doNotAudit == null)
            {
                propertyInfo.SetValue(newEntity, propertyInfo.GetValue(entity));
                return false;
            }

            if (doNotAudit.Nonauditable.Equals(true))
            {
                propertyInfo.SetValue(newEntity, null);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validates the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns> 
        public virtual ValidationResults Validate(T entity)
        {
            ValidationResults results = new ValidationResults();
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(entity, serviceProvider: null, items: null);

            var isValid = Validator.TryValidateObject(entity, context, results);

            results.IsValid = isValid;

            return results;
        }
    }
}
