using System;
using System.Linq;
using System.Threading.Tasks;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Model.Attributes;

namespace OpenBots.Server.WebAPI.Controllers.OrganizationApi
{
    /// <summary>
    /// Controller for User Consent
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class UserConsentController : EntityController<UserConsent>
    {
        readonly IUserConsentRepository repository;
        readonly IMembershipManager membershipManager;
        readonly IPersonRepository personRepository;
        readonly IPersonEmailRepository personEmailRepository;
        readonly IEmailVerificationRepository emailVerificationRepository;

        /// <summary>
        /// UserConsentController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="membershipManager"></param>
        /// <param name="personRepository"></param>
        /// <param name="personEmailRepository"></param>
        /// <param name="emailVerificationRepository"></param>
        public UserConsentController(
            IUserConsentRepository repository,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IMembershipManager membershipManager,
            IPersonRepository personRepository,
            IPersonEmailRepository personEmailRepository,
            IEmailVerificationRepository emailVerificationRepository,
            IConfiguration configuration) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.repository = repository;
            this.membershipManager = membershipManager;
            this.personRepository = personRepository;
            this.personEmailRepository = personEmailRepository;
            this.emailVerificationRepository = emailVerificationRepository;
            this.repository.SetContext(SecurityContext);
            this.membershipManager.SetContext(SecurityContext);
            this.personRepository.SetContext(SecurityContext);           
            this.personEmailRepository.SetContext(SecurityContext);
            this.emailVerificationRepository.SetContext(SecurityContext);
        }

        /// <summary>
        /// Provides a list of all user consents
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok,a paginated list of all user consents</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>        
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all user consents </returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<UserConsent>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<UserConsent> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides a user consent details for a particular organization id.
        /// </summary>
        /// <param name="id">User consent id</param>
        /// <response code="200">Ok if user consent entity exists with the given id.</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if user consent id is not in proper format or proper Guid.</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no user consent entity exists for the given user consent id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>User consent details for the given id</returns>
        [HttpGet("{id}", Name = "GetUserConsent")]
        [ProducesResponseType(typeof(UserConsent), StatusCodes.Status200OK)]
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
                var result = base.GetEntity(id).Result;
                if (result.GetType() == typeof(StatusCodeResult)) { return result; }
                var userConsent = ((OkObjectResult)result).Value as UserConsent;
              
                return Ok(userConsent);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Save user constent
        /// </summary>
        /// <param name="value"></param>
        /// <response code="200">Ok, user constent saved </response>
        /// <response code="400">Bad request, when the user consent value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered.</response>
        /// <returns>Newly created user constent details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(UserConsent), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] UserConsent value)
        {
            if (value == null)
            {
                ModelState.AddModelError("Save", "No data passed");
                return BadRequest(ModelState);
            }
           
            try
            {
                var userConsent = repository.Find(null, p => p.PersonId == SecurityContext.PersonId && p.UserAgreementID == value.UserAgreementID)?.Items?.FirstOrDefault();
                if (userConsent != null)
                {
                    userConsent.IsAccepted = value.IsAccepted;
                    if (value.IsAccepted)
                        userConsent.ExpiresOnUTC = DateTime.UtcNow.AddYears(1);
                    return await base.PutEntity(userConsent.Id.ToString(), userConsent).ConfigureAwait(false);
                }
                else
                {
                    var userAgreementConsent = new UserConsent()
                    {
                        PersonId = value.PersonId,
                        UserAgreementID = value.UserAgreementID,
                        IsAccepted = value.IsAccepted,
                        ExpiresOnUTC = DateTime.UtcNow.AddYears(1)
                    };
                    return await base.PostEntity(userAgreementConsent);
                }
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates user consent entity 
        /// </summary>
        /// <remarks>
        /// Provides an action to update user consent, when user consent id and the new details of user consent are given
        /// </remarks>
        /// <param name="id">User consent id, produces bad request if id is null or ids don't match</param>
        /// <param name="value">User consent details to be updated</param>
        /// <response code="200">Ok, if the user consent details for the given id have been updated.</response>
        /// <response code="400">Bad request, if the user consent id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserConsent), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] UserConsent value)
        {
            if (value == null && string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("Update", "user consent details not passed");
                return BadRequest(ModelState);
            }
            Guid entityId = new Guid(id);
            value.Id = entityId;
            return await base.PutEntity(id, value).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes user consent with a specified id from the database.
        /// </summary>
        /// <param name="id">User consent id to be deleted - throws bad request if null or empty Guid/</param>
        /// <response code="200">Ok, when user consent is soft deleted,(isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if organization id is null or empty Guid</response>
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
            if (string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("Delete", "UserConsent Id not passed");
                return BadRequest(ModelState);
            }

            return await base.DeleteEntity(id);
        }

        /// <summary>
        /// Updates partial details of user consent.
        /// </summary>
        /// <param name="id">user consent identifier</param>
        /// <param name="value">value of the user consent to be updated.</param>
        /// <response code="200">Ok, if update of user consent is successful. </response>
        /// <response code="400">Bad request, if the id is null or ids dont match.</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial user consent values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<UserConsent> value)
        {
            if (value == null && string.IsNullOrEmpty(id))
            {
                ModelState.AddModelError("Update", "User consent details not passed");
                return BadRequest(ModelState);
            }
            return await base.PatchEntity(id, value);
        }
    }
}

