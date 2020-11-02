using System;
using System.Linq;
using System.Threading.Tasks;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.Model.Attributes;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers.Core
{
    /// <summary>
    /// Controller for lookup values
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class LookupValuesController : EntityController<LookupValue>
    {
        /// <summary>
        /// LookupValuesController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        public LookupValuesController(
            ILookupValueRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IOrganizationManager organizationManager,
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor) : base(repository,  userManager, httpContextAccessor, membershipManager, configuration)
        {           
        }

        /// <summary>
        /// Provides a list of all lookup values
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="top"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <response code="200">Ok, a paginated list of all lookup values</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>        
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all lookup values</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<LookupValue>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<LookupValue> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides lookup value details for a particular id
        /// </summary>
        /// <param name="codeType">Lookup value id</param>
        /// <response code="200">Ok, if a lookup value exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if a lookup value id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no lookup value exists for the given lookup value id</response>
        /// <returns>Lookup value details for the given id</returns>
        [HttpGet("{codeType}", Name = "GetLookupValue")]
        [ProducesResponseType(typeof(PaginatedList<LookupValue>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get(string codeType)
        {
            try
            {
                var lookup = repository.Find(null, p => p.CodeType.Equals(codeType, StringComparison.OrdinalIgnoreCase));
                if (lookup == null) {
                    ModelState.AddModelError("", "");
                    return BadRequest(ModelState);
                }

                var orderedItems = lookup.Items.OrderBy(x => x.SequenceOrder).ToList();
                lookup.Items.Clear();
                lookup.Items.AddRange(orderedItems);

                return Ok(lookup);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new lookup value to the existing lookup values
        /// </summary>
        /// <remarks>
        /// Adds the lookup value with unique lookup value id to the existing lookup values
        /// </remarks>
        /// <param name="value"></param>
        /// <response code="200">Ok, new lookup value created and returned</response>
        /// <response code="400">Bad request, when lookup value is not in the proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created unique lookup value details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(LookupValue), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] LookupValue value)
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
                return await base.PostEntity(value);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }


        /// <summary>
        /// Updates a lookup value 
        /// </summary>
        /// <remarks>
        /// Provides an action to update a lookup value, when lookup value id and the new details of lookup value are given
        /// </remarks>
        /// <param name="id">Lookup value id, produces bad request if id is null or ids don't match</param>
        /// <param name="value">lookup value details to be updated</param>
        /// <response code="200">Ok, if the lookup value details for the given lookup value id has been updated</response>
        /// <response code="400">Bad request, if the lookup value id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] LookupValue value)
        {
            return await base.PutEntity(id, value);

        }

        /// <summary>
        /// Deletes a lookup value with a specified id
        /// </summary>
        /// <param name="id">Lookup value id to be soft deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when lookup value is soft deleted,(isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if lookup value id is null or empty Guid</response>
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

        /// <summary>
        /// Updates partial details of a lookup value
        /// </summary>
        /// <param name="id">Lookup value identifier</param>
        /// <param name="value">Value of the lookup value to be updated</param>
        /// <response code="200">Ok, if update of lookup value is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial values have been updated</returns>

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<LookupValue> value)
        {
            return await base.PatchEntity(id, value);
        }
    }
}