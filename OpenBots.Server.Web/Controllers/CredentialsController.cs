using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web
{
    /// <summary>
    /// Controller for credentials
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class CredentialsController : EntityController<Credential>
    {
        private readonly ICredentialManager _credentialManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebhookPublisher _webhookPublisher;

        /// <summary>
        /// CredentialsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="credentialManager"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="webhookPublisher"></param>
        public CredentialsController(
            ICredentialRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            ICredentialManager credentialManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IWebhookPublisher webhookPublisher) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _credentialManager = credentialManager;
            _credentialManager.SetContext(SecurityContext);
            _webhookPublisher = webhookPublisher;
        }

        /// <summary>
        /// Provides a list of all credentials
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of all credentials</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>        
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all credentials</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<Credential>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            try
            {
                return Ok(base.GetMany());
            }
            catch (Exception ex) 
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides a count of credentials 
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ot, total count of credentials</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>        
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all credentials</returns>
        [HttpGet("Count")]
        [ProducesResponseType(typeof(int?), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Count(
            [FromQuery(Name = "$filter")] string filter = "")
        {
            try
            {
                return Ok(base.Count());
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides a credential's details for a particular credential id
        /// </summary>
        /// <param name="id">Credential id</param>
        /// <response code="200">Ok, if a credential exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if credential id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no credential exists for the given Credential id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Credential details for the given id</returns>
        [HttpGet("{id}", Name = "GetCredential")]
        [ProducesResponseType(typeof(Credential), StatusCodes.Status200OK)]
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
                Guid entityId = new Guid(id);

                var existingCredential = repository.GetOne(entityId);
                if (existingCredential == null) return NotFound();

                if (_credentialManager.ValidateRetrievalDate(existingCredential))
                {
                    return await base.GetEntity(id);
                }
                ModelState.AddModelError("Credential", "Current date does not fall withing the start and end date range");
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides a credential's view details for a particular credential id
        /// </summary>
        /// <param name="id">Credential id</param>
        /// <response code="200">Ok, if a credential exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if credential id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no credential exists for the given credential id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Credential view details for the given id</returns>
        [HttpGet("view/{id}")]
        [ProducesResponseType(typeof(CredentialViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> View(string id)
        {
            try
            {
                return await base.GetEntity<CredentialViewModel>(id);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides a credential's password string for a particular credential id
        /// </summary>
        /// <param name="id">Credential id</param>
        /// <response code="200">Ok, if a credential exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if credential id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no credential exists for the given credential id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Credential view details for the given id</returns>
        [HttpGet("password/{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetPassword(string id)
        {
            try
            {
                IActionResult actionResult = await base.GetEntity(id);
                OkObjectResult okResult = actionResult as OkObjectResult;

                if (okResult != null)
                {
                    Credential credential = okResult.Value as Credential;
                    okResult.Value = credential.PasswordSecret;
                }

                return actionResult;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }         
        }

        /// <summary>
        /// Adds a new credential to the existing credentials
        /// </summary>
        /// <remarks>
        /// Adds the Credential with unique id to the existing credentials
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">Ok, new credential created and returned</response>
        /// <response code="400">Bad request, when the credential value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity, when a duplicate record is being entered</response>
        /// <returns>Newly created unique credential</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Credential), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] GlobalCredentialViewModel request)
        {
            try
            {
                applicationUser = userManager.GetUserAsync(_httpContextAccessor.HttpContext.User).Result;

                if (request.PasswordSecret != null && applicationUser != null)
                {
                    request.PasswordHash = userManager.PasswordHasher.HashPassword(applicationUser, request.PasswordSecret);
                }

                Credential credential = _credentialManager.CreateGlobalCredential(request);

                var result = await base.PostEntity(credential);
                await _webhookPublisher.PublishAsync("Credentials.NewCredentialCreated", credential.Id.ToString(), credential.Name).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new Agent Credential if a global Credential exists for the given name
        /// </summary>
        /// <param name="request">New Agent Credential to create</param>
        /// <response code="200">Ok, Credential created and returned</response>
        /// <response code="400">Bad request, when the Credential value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity, when a duplicate record is being entered</response>
        /// <returns>Newly created unique credential</returns>
        [HttpPost("AddAgentCredential")]
        [ProducesResponseType(typeof(Credential), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> AddAgentCredential([FromBody] AgentCredentialViewModel request)
        {
            try
            {
                applicationUser = userManager.GetUserAsync(_httpContextAccessor.HttpContext.User).Result;

                if (request.PasswordSecret != null && applicationUser != null)
                {
                    request.PasswordHash = userManager.PasswordHasher.HashPassword(applicationUser, request.PasswordSecret);
                }

                Credential credential = _credentialManager.CreateAgentCredential(request);

                var result = await base.PostEntity(credential);
                await _webhookPublisher.PublishAsync("Credentials.NewCredentialCreated", credential.Id.ToString(), credential.Name).ConfigureAwait(false);
                return result;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates a credential 
        /// </summary>
        /// <remarks>
        /// Provides an action to update a credential, when id and the new details of credential are given
        /// </remarks>
        /// <param name="id">Credential Id,produces Bad request if Id is null or Id's don't match</param>
        /// <param name="request">Credential details to be updated</param>
        /// <response code="200">Ok, if the credential details for the given credential id have been updated</response>
        /// <response code="400">Bad request, if the credential id is null or ids don't match</response>
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
        public async Task<IActionResult> Put(string id, [FromBody] Credential request)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingCredential = repository.GetOne(entityId);
                if (existingCredential == null)
                {
                    ModelState.AddModelError("Credential", "Credential cannot be found or does not exist.");
                    return NotFound(ModelState);
                }

                var credential = repository.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null) && d.Id != entityId)?.Items?.FirstOrDefault();
                if (credential != null && credential.Id != entityId)
                {
                    ModelState.AddModelError("Credential", "Credential Name Already Exists");
                    return BadRequest(ModelState);
                }

                if (!_credentialManager.ValidateStartAndEndDates(request))
                {
                    ModelState.AddModelError("Credential", "Start and End Date are not valid");
                    return BadRequest(ModelState);
                }

                existingCredential.Provider = request.Provider;
                existingCredential.StartDate = request.StartDate;
                existingCredential.EndDate = request.EndDate;
                existingCredential.Domain = request.Domain;
                existingCredential.UserName = request.UserName;
                existingCredential.PasswordSecret = request.PasswordSecret;
                existingCredential.PasswordHash = request.PasswordHash;
                existingCredential.Certificate = request.Certificate;

                applicationUser = userManager.GetUserAsync(_httpContextAccessor.HttpContext.User).Result;

                if (request.PasswordSecret != existingCredential.PasswordSecret && applicationUser != null)
                {
                    existingCredential.PasswordHash = userManager.PasswordHasher.HashPassword(applicationUser, request?.PasswordSecret);
                }

                await _webhookPublisher.PublishAsync("Credentials.CredentialUpdated", existingCredential.Id.ToString(), existingCredential.Name).ConfigureAwait(false);
                return await base.PutEntity(id, existingCredential);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Deletes a credential with a specified id
        /// </summary>
        /// <param name="id">Credential id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when credential is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if credential id is null or empty Guid</response>
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
            try
            {
                var credential = _credentialManager.DeleteCredential(id);

                await _webhookPublisher.PublishAsync("Credentials.CredentialDeleted", credential.Id.ToString(), credential.Name).ConfigureAwait(false);
                return await base.DeleteEntity(id);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates partial details of credential
        /// </summary>
        /// <param name="id">Credential identifier</param>
        /// <param name="request">Value of the credential to be updated</param>
        /// <response code="200">Ok, if update of credential is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial credential values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<Credential> request)
        {
            try
            {
                Guid entityId = new Guid(id);
                var existingCredential = repository.GetOne(entityId);
                if (existingCredential == null)
                {
                    ModelState.AddModelError("Credential", "Credential cannot be found or does not exist.");
                    return NotFound(ModelState);
                }

                for (int i = 0; i < request.Operations.Count; i++)
                {
                    //verify that credential name is not taken
                    if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/name")
                    {
                        existingCredential.Name = request.Operations[i].value.ToString();
                        _credentialManager.CredentialNameAvailability(existingCredential);
                    }

                    //generate new password hash
                    if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/passwordsecret")
                    {
                        applicationUser = userManager.GetUserAsync(_httpContextAccessor.HttpContext.User).Result;

                        var passwordHash = userManager.PasswordHasher.HashPassword(applicationUser, request.Operations[i].value.ToString());
                        request.Replace(e => e.PasswordHash, passwordHash);
                    }

                    //verify start-end date range
                    if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/startdate"
                        | request.Operations[i].path.ToString().ToLower() == "/enddate")
                    {
                        if (request.Operations[i].path.ToString().ToLower() == "/startdate")
                        {
                            existingCredential.StartDate = Convert.ToDateTime(request.Operations[i].value.ToString());
                        }
                        else
                        {
                            existingCredential.EndDate = Convert.ToDateTime(request.Operations[i].value.ToString());
                        }

                        if (!_credentialManager.ValidateStartAndEndDates(existingCredential))
                        {
                            ModelState.AddModelError("Credential", "Start and End Date are not valid. End Date must be after the Start Date");
                            return BadRequest(ModelState);
                        }
                    }
                }

                await _webhookPublisher.PublishAsync("Credentials.CredentialUpdated", existingCredential.Id.ToString(), existingCredential.Name).ConfigureAwait(false);
                return await base.PatchEntity(id, request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Lookup list of all active directory credentials
        /// </summary>
        /// <response code="200">Ok, a lookup list of all active directory credentials</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Lookup list of all active directory credentials</returns>
        [HttpGet("GetLookup")]
        [ProducesResponseType(typeof(List<CredentialsLookup>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetLookup()
        {
            try
            {
                var credentialList = repository.Find(null, x => x.IsDeleted == false && x.Provider == "AD"); //"AD" is to get all active directory credentials
                var credentialLookup = from a in credentialList.Items.GroupBy(p => p.Id).Select(p => p.First()).ToList()
                                       select new CredentialsLookup
                                       {
                                           CredentialId = (a == null || a.Id == null) ? Guid.Empty : a.Id.Value,
                                           CredentialName = a?.Name
                                       };

                return Ok(credentialLookup.ToList());
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}
