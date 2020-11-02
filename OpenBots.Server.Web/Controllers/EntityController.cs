using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenBots.Server.DataAccess.Exceptions;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers
{
    /// <summary>
    /// Controller for entities
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EntityController<T> : ReadOnlyEntityController<T>
        where T : class, IEntity, new()
    {
        protected readonly IEntityRepository<T> repository;
        
        /// <summary>
        /// EntityController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="membershipManager"></param>
        protected EntityController(IEntityRepository<T> repository, 
          
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IMembershipManager membershipManager,
            IConfiguration configuration) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.repository = repository;
            repository.SetContext(SecurityContext);
        }
 
        /// <summary>
        /// Post action: save entity in database
        /// </summary>
        /// <param name="value"></param>
        /// <param name="resultRoute"></param>
        /// <returns>Saved entity details</returns>
        /// <exception cref="EntityValidationException"></exception>
        /// <exception cref="CannotInsertDuplicateConstraintException"></exception>
        /// <exception cref="EntityAlreadyExistsException"></exception>
        /// <exception cref="EntityConcurrencyException"></exception>
        /// <exception cref="EntityOperationException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        protected virtual async Task<IActionResult> PostEntity(T value, string resultRoute = "")
        {
            if (value == null)
            {
                ModelState.AddModelError("Save", "No data passed");
                return BadRequest(ModelState);
            }

            Guid entityId = Guid.NewGuid();
            if (value.Id == null || value.Id.HasValue || value.Id.Equals(Guid.Empty))
                value.Id = entityId;

            try
            {
                value.CreatedBy = applicationUser?.UserName;
                value.CreatedOn = DateTime.UtcNow;
                repository.Add(value);
                //string path = Request.Path + "/{id}";
                if (string.IsNullOrEmpty(resultRoute))
                    resultRoute = "Get" + typeof(T).Name;

                return CreatedAtRoute(resultRoute, new { id = value.Id.Value.ToString("b") }, value);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Put action: update entity in database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns>Updated entity</returns>
        /// <exception cref="EntityValidationException"></exception>
        /// <exception cref="EntityDoesNotExistException"></exception>
        /// <exception cref="EntityConcurrencyException"></exception>
        /// <exception cref="EntityOperationException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        protected virtual async Task<IActionResult> PutEntity(string id, T value)
        {
            if (value == null && string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("Update", "ID is null");
                return BadRequest(ModelState);
            }
            Guid entityId = new Guid(id);

            if (value == null || value.Id == null || !value.Id.HasValue || entityId != value.Id.Value)
            {
                ModelState.AddModelError("Update", "IDs Dont Match");
                return BadRequest(ModelState);
            }

            byte[] timestamp = null;

            if (Request.Headers.ContainsKey("if-match"))
            {
                string etag = Request.Headers["if-match"];
                etag = etag.Trim('"');
                timestamp = Convert.FromBase64String(etag);
            }

            try
            {
                value.UpdatedBy = string.IsNullOrWhiteSpace(applicationUser?.Name)? value.UpdatedBy : applicationUser?.Name;
                repository.Update(value, timestamp);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Patch action: update entity detail
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <exception cref="EntityDoesNotExistException"></exception>
        /// <exception cref="EntityOperationException"></exception>
        /// <exception cref="EntityConcurrencyException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        /// <exception cref="EntityValidationException"></exception>
        /// <returns>Ok response</returns>
        protected virtual async Task<IActionResult> PatchEntity(string id, JsonPatchDocument<T> value)
        {
            if (value == null || string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("Patch", "ID is null");
                return BadRequest(ModelState);
            }
            Guid entityId = new Guid(id);

            byte[] timestamp = null;

            if (Request.Headers.ContainsKey("if-match"))
            {
                string etag = string.Empty;
                etag = Request.Headers["if-match"];
                etag = etag.Trim('"');
                timestamp = Convert.FromBase64String(etag);
            }

            try
            {
                T entity = repository.GetOne(entityId);

                List<string> errors = new List<string>();
                value.ApplyTo(entity, e =>
                {
                    errors.Add(e.ErrorMessage);
                }
                );
                if (errors.Count > 0)
                {
                    ValidationProblemDetails vpd = new ValidationProblemDetails();
                    vpd.Errors.Add(typeof(T).Name, errors.ToArray());
                    return ValidationProblem(vpd);
                }

                repository.Update(entity, timestamp);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Delete action: soft delete from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Ok response</returns>
        /// <exception cref="EntityDoesNotExistException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        /// <exception cref="EntityOperationException"></exception>
        protected virtual async Task<IActionResult> DeleteEntity(string id)
        {
            Guid entityId = new Guid(id);
            if (entityId == null || Guid.Empty.Equals(entityId))
            {
                ModelState.AddModelError("Delete", "ID is null");
                return BadRequest(ModelState);
            }

            try
            {
                repository.SoftDelete(entityId);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}
