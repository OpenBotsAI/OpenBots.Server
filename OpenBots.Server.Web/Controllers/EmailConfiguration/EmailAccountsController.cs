using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.WebAPI.Controllers;

namespace OpenBots.Server.Web.Controllers.EmailConfiguration
{
    /// <summary>
    /// Controller for email accounts
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailAccountsController : EntityController<EmailAccount>
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        
        /// <summary>
        /// EmailLogsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        public EmailAccountsController(
            IEmailAccountRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.httpContextAccessor = httpContextAccessor;    
        }

        /// <summary>
        /// Provides a list of all email accounts
        /// </summary>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <param name="orderBy"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a paginated list of all email accounts</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all email accounts</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<EmailAccountViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<EmailAccountViewModel> Get(
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0
        )
        {
            return base.GetMany<EmailAccountViewModel>();
        }

        /// <summary>
        /// Gets count of processes in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a count of all email accounts</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable etity</response>
        /// <returns>Count of all email accounts</returns>
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
        /// Get email account by id
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if an email account exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if email account id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no email account exists for the given email account id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Email account details</returns>
        [HttpGet("{id}", Name = "GetEmailAccount")]
        [ProducesResponseType(typeof(PaginatedList<EmailAccount>), StatusCodes.Status200OK)]
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
        /// Adds a new email account to the existing email accounts
        /// </summary>
        /// <remarks>
        /// Adds the email account with unique email account id to the existing email accounts
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">Ok, new email account created and returned</response>
        /// <response code="400">Bad request, when the email account value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns> Newly created unique email account </returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmailAccount), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] EmailAccount request)
        {
            try
            {
                var emailAccount = repository.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null))?.Items?.FirstOrDefault();
                if (emailAccount != null)
                {
                    ModelState.AddModelError("Email Account", "Email account already exists");
                    return BadRequest(ModelState);
                }

                applicationUser = userManager.GetUserAsync(httpContextAccessor.HttpContext.User).Result;
                if (request.EncryptedPassword != null && applicationUser != null)
                    request.PasswordHash = userManager.PasswordHasher.HashPassword(applicationUser, request.EncryptedPassword);

                return await base.PostEntity(request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates an email account
        /// </summary>
        /// <remarks>
        /// Provides an action to update an email account, when email account id and the new details of email account are given
        /// </remarks>
        /// <param name="id">Email account id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Email account details to be updated</param>
        /// <response code="200">Ok, if the email account details for the given email account id have been updated.</response>
        /// <response code="400">Bad request, if the email account id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EmailAccount), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] EmailAccount request)
        {
            try 
            {
                Guid entityId = new Guid(id);

                var existingEmailAccount = repository.GetOne(entityId);
                if (existingEmailAccount == null) return NotFound();

                var emailAccount = repository.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null) && d.Id != entityId)?.Items?.FirstOrDefault();
                if (emailAccount != null && emailAccount.Id != entityId)
                {
                    ModelState.AddModelError("Email Account", "Email Account already exists");
                    return BadRequest(ModelState);
                }

                existingEmailAccount.Name = request.Name;
                existingEmailAccount.IsDisabled = request.IsDisabled;
                existingEmailAccount.IsDefault = request.IsDefault;
                existingEmailAccount.Provider = request.Provider;
                existingEmailAccount.IsSslEnabled = request.IsSslEnabled;
                existingEmailAccount.Host = request.Host;
                existingEmailAccount.Port = request.Port;
                existingEmailAccount.Username = request.Username;
                existingEmailAccount.EncryptedPassword = request.EncryptedPassword;
                existingEmailAccount.PasswordHash = request.PasswordHash;
                existingEmailAccount.ApiKey = request.ApiKey;
                existingEmailAccount.FromEmailAddress = request.FromEmailAddress;
                existingEmailAccount.FromName = request.FromName;
                existingEmailAccount.StartOnUTC = request.StartOnUTC;
                existingEmailAccount.EndOnUTC = request.EndOnUTC;

                applicationUser = userManager.GetUserAsync(httpContextAccessor.HttpContext.User).Result;
                if (request.EncryptedPassword != null && applicationUser != null)
                    existingEmailAccount.PasswordHash = userManager.PasswordHasher.HashPassword(applicationUser, request.EncryptedPassword);

                return await base.PutEntity(id, existingEmailAccount);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Email Account", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates partial details of an email account.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <response code="200">Ok, if update of email account is successful</response>
        /// <response code="400">Bad request, if the id is null or ids dont match.</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(EmailAccount), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch (string id, [FromBody] JsonPatchDocument<EmailAccount> request)
        {
            return await base.PatchEntity(id, request);
        }

        /// <summary>
        /// Delete email account with a specified id from list of email accounts
        /// </summary>
        /// <param name="id">Email account id to be deleted - throws  bad request if null or empty Guid/</param>
        /// <response code="200">Ok, when email account is soft deleted,(isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if email account id is null or empty Guid</response>
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
        /// Lookup list of all email accounts
        /// </summary>
        /// <response code="200">Ok, a lookup list of all email accounts</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Lookup list of all email accounts</returns>
        [HttpGet("GetLookup")]
        [ProducesResponseType(typeof(List<EmailAccountLookup>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public List<EmailAccountLookup> GetLookup()
        {
            var accountList = repository.Find(null, x => x.IsDeleted == false);
            var accountLookup = from a in accountList.Items.GroupBy(p => p.Id).Select(p => p.First()).ToList()
                                   select new EmailAccountLookup
                                   {
                                       EmailAccountId = (a == null || a.Id == null) ? Guid.Empty : a.Id.Value,
                                       EmailAccountName = a?.Name
                                   };

            return accountLookup.ToList();
        }
    }
}
