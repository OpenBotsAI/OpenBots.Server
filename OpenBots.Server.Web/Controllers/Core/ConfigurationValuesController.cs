using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.WebAPI.Controllers;

namespace OpenBots.Server.Web.Controllers.Core
{
    /// <summary>
    /// Controller for Configuration Values
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class ConfigurationValuesController : EntityController<ConfigurationValue>
    {

        /// <summary>
        /// ConfigurationValues constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="membershipManager"></param>
        /// <param name="configuration"></param>
        public ConfigurationValuesController(
            IConfigurationValueRepository repository,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IMembershipManager membershipManager,
            IConfiguration configuration
            ) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        { }

        /// <summary>
        /// Provides a list of all configuration values
        /// </summary>
        /// <response code="200">Ok, a Paginated list of all configuration values</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>        
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all configuration values</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<ConfigurationValue>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<ConfigurationValue> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Gets count of configuration values in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a count of all configuration values</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of configuration values</returns>
        [HttpGet("count")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<int?> GetCount(
        [FromQuery(Name = "$filter")] string filter = "")
        {
            return base.Count();
        }

        /// <summary>
        /// Get configuration value by id
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if a cofiguration value exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if configuration value id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no configuration value exists for the given email setting id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Configuration value details</returns>
        [HttpGet("{id}", Name = "GetConfigurationValue")]
        [ProducesResponseType(typeof(PaginatedList<ConfigurationValue>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                return await base.GetEntity(id);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new configuration value to the existing configuration values
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, new confiuration value created and returned</response>
        /// <response code="400">Bad request, when the configuration value's value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created unique configuration value</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ConfigurationValue), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] ConfigurationValue request)
        {
            try
            {
                var configurationValue = repository.Find(null, q => q.Name == request.Name)?.Items?.FirstOrDefault();
                if (configurationValue != null && configurationValue.IsDeleted == false)
                {
                    ModelState.AddModelError("ConfigurationValue", "Configuration setting already exists.  Please update existing settings.");
                    return BadRequest(ModelState);
                }

                return await base.PostEntity(request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates configuration value
        /// </summary>
        /// <remarks>
        /// Provides an action to update a configuration value, when configuration value id and the new details of configuration value are given
        /// </remarks>
        /// <param name="id">Configuration value id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Configuration value details to be updated</param>
        /// <response code="200">Ok, if the configuration setting details for the given configuration value id have been updated.</response>
        /// <response code="400">Bad request, if the configuration value id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ConfigurationValue), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] ConfigurationValue request)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingConfigurationValue = repository.GetOne(entityId);
                if (existingConfigurationValue == null) return NotFound();

                //existingConfigurationValue.Name = request.Name;
                existingConfigurationValue.Description = request.Description;
                existingConfigurationValue.UIHint = request.UIHint;
                existingConfigurationValue.ValidationRegex = request.ValidationRegex;
                existingConfigurationValue.Value = request.Value;

                return await base.PutEntity(id, existingConfigurationValue);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Settings", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates partial details of configuration value
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <response code="200">Ok, if update of configuration value is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(EmailSettings), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody] JsonPatchDocument<ConfigurationValue> request)
        {
            return await base.PatchEntity(id, request);
        }

        /// <summary>
        /// Delete configuration value with a specified id from configuration value data table
        /// </summary>
        /// <param name="id">Configuration value id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when configuration value is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if configuration value id is null or empty Guid</response>
        /// <response code="403">Forbidden</response>
        /// <returns>Ok response</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete(string id)
        {
            return await base.DeleteEntity(id);
        }
    }
}
