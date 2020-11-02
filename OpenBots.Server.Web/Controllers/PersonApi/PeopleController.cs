using System.Threading.Tasks;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Business;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Authorization;
using OpenBots.Server.Model.Attributes;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers
{
    /// <summary>
    /// Controller for users (people)
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class PeopleController : EntityController<Person>
    {
        /// <summary>
        /// PeopleController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="membershipManager"></param>
        /// <param name="configuration"></param>
        public PeopleController(
            IPersonRepository repository, 
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IMembershipManager membershipManager,
            IConfiguration configuration) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        { }

        /// <summary>
        /// Gets all the users (people) 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <response code="200">Ok, a paginated list of all people</response>
        /// <response code="400">Bad request, f it is not a proper Guid</response>
        /// <response code="403">Forbidden, person id passed in the URL is not the same as the currently logged in user</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Paginated list of all the users (people)</returns>
        [HttpGet]
        [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public PaginatedList<Person> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            return base.GetMany();
        }

        /// <summary>
        /// Retrieves the person details for a particular user
        /// </summary>
        /// <param name="id">People identifier</param>
        /// <response code="200">Ok, if person details are available for the given id></response>
        /// <response code="400">Bad request, if the person id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, person id passed in the URL is not the same as the currently logged in user</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with person details</returns> 
        [HttpGet("{id}", Name = "GetPerson")]
        [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string id)
        {
            return await base.GetEntity(id);
        }

        /// <summary>
        /// Adds a person
        /// </summary>
        /// <param name="value"></param>
        /// <response code="200">Ok, if the person details have been added</response>
        /// <response code="400">Bad request, if an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="409">Conflict, concurrency error</response>
        /// <response code="422">Unprocessable entity, if a duplicate record is being entered (person record already exists)</response> 
        /// <returns>Ok response with person details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Person), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Post([FromBody] Person value)
        {
            return await base.PostEntity(value);
        }

        /// <summary>
        /// Updates person details
        /// </summary>
        /// <param name="id">Person id</param>
        /// <param name="value">Details to be updated</param>
        /// <response code="200">Ok, if the details of person have been updated</response>
        /// <response code="400">Bad request, if person id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="409">Conflict, concurrency error</response>
        /// <response code="422">Unprocessable entity, validation error.</response>
        /// <returns>Ok response with updated person details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Put(string id, [FromBody] Person value)
        {
            //Use logged in User Context
            if (applicationUser == null)
            {
                return Unauthorized();
            }

            var existingPerson = repository.GetOne(applicationUser.PersonId);
            if (existingPerson == null) return NotFound();
            
            existingPerson.Name = value.Name;

            applicationUser.Name = value.Name;
            await userManager.UpdateAsync(applicationUser).ConfigureAwait(false);           

            return await base.PutEntity(id, existingPerson).ConfigureAwait(false);
        }   

        /// <summary>
        /// Deletes the person details
        /// </summary>
        /// <param name="id">Person id</param>
        /// <response code="200">Ok, if the person details for the particular id have been soft deleted</response>
        /// <response code="400">Bad request, if person id is not provided or an improper guid</response>
        /// <response code="403">Forbidden, person id passed in the URL is not the same as the currently logged in user</response>
        /// <returns>Ok response, if the person details for the particular id have been soft deleted</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(string id)
        {
            return await base.DeleteEntity(id);
        }

        /// <summary>
        /// Updates person details 
        /// </summary>
        /// <param name="id">People identifier</param>
        /// <param name="value">Values to be updated</param>
        /// <response code="200">Ok, if update of person details are successful</response>
        /// <response code="400">Bad request, if the id is null or ids dont match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial person values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody]JsonPatchDocument<Person> value)
        {
            return await base.PatchEntity(id, value);
        }
    }
}