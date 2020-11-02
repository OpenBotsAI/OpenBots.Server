using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Controller for people's emails
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/People/{personId}/[controller]")]
    [ApiController]
    [Authorize]
    public class PersonEmailsController : EntityController<PersonEmail>
    {
        /// <summary>
        /// PersonEmailsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        public PersonEmailsController(
            IPersonEmailRepository repository, 
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        { }

        /// <summary>
        /// Gets all the email address and verification id for a particular person
        /// </summary>
        /// <param name="personId">Person identifier</param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="top">returns the top 100</param>
        /// <param name="skip"></param>
        /// <response code="200">Ok, paginated list of all email addresses</response>
        /// <response code="400">Bad request, if person id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, person id passed in the URL is not the same as the currently logged in user</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Paginated list of all the email addresses for a particular person</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PersonEmail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public PaginatedList<PersonEmail> Get(
            [FromRoute] string personId,
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            return base.GetMany(personId);
        }

        /// <summary>
        /// Gets the email address and verfiication id for a particular email for person
        /// </summary>
        /// <param name="id">Email identifier</param>
        /// <response code="200">Ok, if email detail is available for the given id></response>
        /// <response code="400">Bad request, if the email id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, person if passed in the URL is not the same as the currently logged in user</response>
        /// <response code="404">NotFound</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, email details</returns>
        [HttpGet("{id}", Name = "GetPersonEmail")]
        [ProducesResponseType(typeof(PersonEmail), StatusCodes.Status200OK)]
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
        /// Adds a new email id and verification id for a person
        /// </summary>
        /// <param name="personId">Person identifier</param>
        /// <param name="value">Email details to be added</param>
        /// <response code="200">Ok, if the email details have been added</response>
        /// <response code="400">Bad request, if person id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="409">Conflict, concurrency error</response>
        /// <response code="422">Unprocessable entity, if a duplicate record is being entered, email detials already exists</response> 
        /// <returns>Ok response with newly added email details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(PersonEmail), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Post(string personId, [FromBody] PersonEmail value)
        {
            value.PersonId = new Guid(personId);
            return await base.PostEntity(value);
        }

        /// <summary>
        /// Updates the email details for person
        /// </summary>
        /// <param name="personId">Person identifier</param>
        /// <param name="id">Email identifier</param>
        /// <param name="value">Value to be updated</param>
        /// <response code="200">Ok, if the details of email have been updated</response>
        /// <response code="400">Bad request, if person id or email id is not provided or an improper Guid.</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="409">Conflict, concurrency error</response>
        /// <response code="422">Unprocessable entity, validation error.</response>
        /// <returns>Ok response with newly updated email details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Put(string personId, string id, [FromBody] PersonEmail value)
        {
            value.PersonId = new Guid(personId);
            return await base.PutEntity(id, value);
        }

        /// <summary>
        /// Deletes the email details 
        /// </summary>
        /// <param name="id">Email identifier</param>
        /// <response code="200">Ok, if the email details for the particular id have been deleted</response>
        /// <response code="400">Bad request, if email id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <returns>Ok response if the soft delete is successful</returns>
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
        /// Updates partial details of email
        /// </summary>
        /// <param name="id">Person email identifier</param>
        /// <param name="value">Value to be updated</param>
        /// <response code="200">Ok, if update of email details are successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial email values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody]JsonPatchDocument<PersonEmail> value)
        {
            return await base.PatchEntity(id, value);
        }
    }
}