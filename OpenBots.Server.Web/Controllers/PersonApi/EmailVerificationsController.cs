using System;
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
using System.IO;
using OpenBots.Server.Model.Attributes;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers
{
    /// <summary>
    /// Controller for email verifications
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/People/{personId}/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailVerificationsController : EntityController<EmailVerification>
    {
        private readonly IEmailManager emailSender;

        /// <summary>
        /// EmailVerificationsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        public EmailVerificationsController(
            IEmailVerificationRepository repository, 
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IEmailManager emailSender) : base(repository,  userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.emailSender = emailSender;
        }

        /// <summary>
        /// Gets all the email verfication details for a particular person 
        /// </summary>
        /// <param name="personId">Person identifier</param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <response code="200">Ok, paginated list of the email verfications for a particular person</response>
        /// <response code="400">Bad request, if the person id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Paginated list of all the email verfications for a particular person</returns>
        /// <returns>All email verifications for a particular person</returns>
        [HttpGet]
        [ProducesResponseType(typeof(EmailVerification), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<EmailVerification> Get(
            [FromRoute] string personId,
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            return base.GetMany(personId);
        }

        /// <summary>
        /// Provides the email verification details for a particular id
        /// </summary>
        /// <param name="id">Email verfication id</param>
        /// <response code="200">Ok, if email verfication detail is available with the given id></response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if the id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with email verification details</returns>
        [HttpGet("{id}", Name = "GetEmailVerification")]
        [ProducesResponseType(typeof(EmailVerification), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get(string id)
        {
            return await base.GetEntity(id);
        }

        /// <summary>
        /// Adds email verfication details for a person
        /// </summary>
        /// <param name="personId">Person identifier</param>
        /// <param name="value">Value of the email verfication details to be added</param>
        /// <response code="200">Ok, if the email verfication has been added successfully for a person</response>
        /// <response code="400">Bad request, if the person id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="409">Conflict, concurrency error </response>
        /// <response code="422">Unprocessable entity, if a duplicate record is being entered</response> 
        /// <returns>Ok response with email verification details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmailVerification), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post(string personId, [FromBody] EmailVerification value)
        {
            value.PersonId = new Guid(personId);
            //Check the email address in verification email table
            var emailVerification = repository.Find(null, p=> p.Address.Equals(value.Address, StringComparison.OrdinalIgnoreCase))?.Items?.FirstOrDefault();
            
            //If not null then email address already exists
            if (emailVerification != null) {
                ModelState.AddModelError("EmailAddress", "email address already in use");
                return BadRequest(ModelState);
            }

            value.IsVerified = false;
            try
            {
                //Resending 
                byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
                byte[] key = applicationUser.PersonId.ToByteArray();
                string token = Convert.ToBase64String(time.Concat(key).ToArray());

                string confirmationLink = Url.Action("ConfirmEmailAddress",
                                                    "Auth", new
                                                    {
                                                        emailAddress = value.Address,
                                                        token = token
                                                    }, protocol: HttpContext.Request.Scheme);

                string emailBody = "";

                using (StreamReader reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Email/confirm-en.html")))
                {
                    emailBody = reader.ReadToEnd();
                    emailBody = emailBody.Replace("^Confirm^", confirmationLink);
                }

                var subject = "Confirm your email address at " + Constants.PRODUCT;

                bool isEmailAllowed = emailSender.IsEmailAllowed();
                if (isEmailAllowed)
                {
                    EmailMessage emailMessage = new EmailMessage();
                    EmailAddress address = new EmailAddress(value.Person.Name, value.Address);
                    emailMessage.To.Add(address);
                    emailMessage.Body = emailBody;
                    emailMessage.Subject = subject;
                    await emailSender.SendEmailAsync(emailMessage).ConfigureAwait(false);

                    value.IsVerificationEmailSent = true;
                }
                else value.IsVerificationEmailSent = false;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
            var verificationEmail = await base.PostEntity(value).ConfigureAwait(false);
            return verificationEmail;
        }

        /// <summary>
        /// Updates the email verfication details for a person
        /// </summary>
        /// <param name="personId">Person identifier</param>
        /// <param name="id">Email verfication identifier</param>
        /// <param name="value">Value to be updated</param>
        /// <response code="200">Ok, if the email verification value has been updated for the person</response>
        /// <response code="400">Bad request, if the persion id or email verification id is not provided or an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error.</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with updated email verification details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string personId, string id, [FromBody] EmailVerification value)
        {
            value.PersonId = new Guid(personId);
            return await base.PutEntity(id, value);
        }

        /// <summary>
        /// Deletes email verification details
        /// </summary>
        /// <param name="id">Email verification identifier</param>
        /// <response code="200">Ok, if the email verification with the given id has been deleted</response>
        /// <response code="400">Bad request, if the id is not provided or not a proper Guid</response>
        /// <response code="403">Unauthorized access, if the user doesn't have permission to delete the verification details</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the email verification with the given id has been soft deleted</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete(string id)
        {
            return await base.DeleteEntity(id);
        }

        /// <summary>
        /// Updates a portion of email verification details
        /// </summary>
        /// <param name="id">Email verification identifier</param>
        /// <param name="value">Value to be updated</param>
        /// <response code="200">Ok, if update of values of email verfication are successful</response>
        /// <response code="400">Bad request, if the id is null or ids dont match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial email verification values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody]JsonPatchDocument<EmailVerification> value)
        {
            return await base.PatchEntity(id, value);
        }
    }
}