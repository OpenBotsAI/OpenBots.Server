using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Model.Membership;
using OpenBots.Server.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.IO;
using OpenBots.Server.Model.Attributes;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers
{
    /// <summary>
    /// Controller for organization members
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/Organizations/{organizationId}/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizationMembersController : EntityController<OrganizationMember>
    {

        IMembershipManager membershipManager;
        readonly ApplicationIdentityUserManager userManager;
        readonly IPersonRepository personRepository;
        readonly IEmailManager emailSender;
        readonly IAccessRequestsManager accessRequestManager;

        /// <summary>
        /// OrganizationMembersController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="personRepository"></param>
        /// <param name="accessRequestManager"></param>
        /// <param name="emailSender"></param>
        public OrganizationMembersController(IOrganizationMemberRepository repository, 
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IPersonRepository personRepository,
            IAccessRequestsManager accessRequestManager,
            IConfiguration configuration,
            IEmailManager emailSender) : base(repository,  userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.membershipManager = membershipManager;
            this.userManager = userManager;
            this.personRepository = personRepository;
            this.emailSender = emailSender;
            this.accessRequestManager = accessRequestManager;
            this.membershipManager.SetContext(SecurityContext);
            this.personRepository.SetContext(SecurityContext);
            this.accessRequestManager.SetContext(SecurityContext);
        }

        /// <summary>
        /// Gets the people in the organization
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <response code="200">Ok, if all the organization members for the organization have been returned</response>
        /// <response code="400">Bad request, if the organization id is not provided or is an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="404">Not found, no organization member exists for the given organization</response>
        /// <response code="422">Unprocessable entity, validation error</response>        
        /// <returns>Paginated list of team members in the organization</returns>
        [HttpGet("People")]
        [ProducesResponseType(typeof(PaginatedList<TeamMemberViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult>  GetPeople(
            [FromRoute] string organizationId,
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            try
            {
                ODataHelper<TeamMemberViewModel> oData = new ODataHelper<TeamMemberViewModel>();
                string queryString = "";

                if (HttpContext != null
                    && HttpContext.Request != null
                    && HttpContext.Request.QueryString != null
                    && HttpContext.Request.QueryString.HasValue)
                    queryString = HttpContext.Request.QueryString.Value;

                var newNode = oData.ParseOrderByQuerry(queryString);
                if (newNode == null)
                    newNode = new OrderByNode<TeamMemberViewModel>();

                Guid orgId = Guid.Parse(organizationId);
                var result =  membershipManager.GetPeopleInOrganization(orgId, newNode.PropertyName, newNode.Direction, skip, top);                

                return Ok(result);
            }
            catch(Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Gets all the members of the given organization
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <response code="200">Ok, if all the members for the organization have been returned</response>
        /// <response code="400">Bad request, if the organization id is not provided or is an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by user</response>
        /// <response code="404">Not found, no member exists for the given organization id</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Paginated list of all the members of an organization.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(OrganizationMember),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public PaginatedList<OrganizationMember> Get(
            [FromRoute] string organizationId,
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            return base.GetMany(organizationId);
        }

        /// <summary>
        /// Get the member for a particular organization
        /// </summary>
        /// <param name="id">Organization member identifier</param>
        /// <response code="200">Ok, if organization member exists for the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if the organization id is not provided or is an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found, organization member with the particular id does not exist</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with organization member details</returns>
        [HttpGet("{id}", Name = "GetOrganizationMember")]
        [ProducesResponseType(typeof(OrganizationMember),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> Get(string id)
        {
            return await base.GetEntity(id);
        }

        /// <summary>
        /// Adds a new member to the organization
        /// </summary>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="value">Organization member details</param>
        /// <response code="200">Ok, if the member has been added successfully</response>
        /// <response code="400">Bad request, if the organization id is not provided or is an improper Guid</response>
        /// <response code="403">Forbidden, unauthorized access by the user</response>
        /// <response code="409">Conflict, when an organization member with the particular id already exists</response>
        /// <response code="422">Unprocessable entity, validation error or cannot insert duplicate constraint</response>
        /// <returns>Ok response with newly created member details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(OrganizationMember), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> Post(string organizationId, [FromBody] OrganizationMember value)
        {
            value.OrganizationId = new Guid(organizationId);
            return await base.PostEntity(value);
        }

        /// <summary>
        /// Update the organization member details
        /// </summary>
        /// <remarks>Updates the organization member details with the particular id for the given organization</remarks>
        /// <param name="organizationId">Organization identifier</param>
        /// <param name="id">Organization member id</param>
        /// <param name="value">New value of the organization member to be updated</param>
        /// <response code="200">Ok, if the update of the organization member for the particular id has been successful</response>
        /// <response code="400">Bad request, if the id is not provided or Guid is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access by the user</response>
        /// <response code="404">Not found, if no organization member exists for the given id</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with updated organization member details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> Put(string organizationId, string id, [FromBody] OrganizationMember value)
        {
            value.OrganizationId = new Guid(organizationId);
            return await base.PutEntity(id, value);
        }

        /// <summary>
        /// Invite user to become an organization member
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="value"></param>
        /// <response code="200">Ok, if the invite of the organization member has been successful</response>
        /// <response code="400">Bad request, if the organization id is not provided or Guid is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access by the user</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response</returns>
        [HttpPost("InviteUser")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> InviteUser(string organizationId, [FromBody] InviteUserViewModel value)
        {          
            OrganizationMember teamMember; 
            value.OrganizationId = new Guid(organizationId);
            var user = new ApplicationUser();

            //Add person to organization only if you are admin or add it to access request table
            var requestingUser = repository.Find(null, a => a.PersonId == SecurityContext.PersonId && a.OrganizationId == Guid.Parse(organizationId))?.Items.FirstOrDefault();
            var isRequestingUserAdministrator = requestingUser.IsAdministrator.GetValueOrDefault(false);

            // If the requesting user is NOT an Administrator then the user cannot skip Email Verification
            // Only Administrators can allow that. However this can be skipped for now
            //if (value.SkipEmailVerification && !isRequestingUserAdministrator)
            //    value.SkipEmailVerification = false;

            try
            {
                bool IsEmailAllowed = emailSender.IsEmailAllowed();

                var organizationMember = repository.Find(null, a => a.PersonId == SecurityContext.PersonId && a.OrganizationId == Guid.Parse(organizationId))?.Items.FirstOrDefault();
                if (organizationMember == null)
                {
                    throw new UnauthorizedAccessException();
                }

                //This is to check if the user is already in the system and where is part of the organization
                teamMember = membershipManager.InviteUser(value, SecurityContext);
                if (teamMember == null)
                {
                    user.UserName = value.Email;
                    user.Email = value.Email;
                    string passwordString = value.Password;

                    if (IsEmailAllowed)
                    {
                        if (string.IsNullOrEmpty(passwordString))
                        {
                            RandomPassword randomPass = new RandomPassword();
                            passwordString = randomPass.GenerateRandomPassword();
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(passwordString))
                        {
                            ModelState.AddModelError("Invite User", "Email is disabled.  Must provide a password.");
                            return BadRequest(ModelState);
                        }
                    }

                    var loginResult = await userManager.CreateAsync(user, passwordString).ConfigureAwait(false);

                    if (!loginResult.Succeeded)
                    {
                        return GetErrorResult(loginResult);
                    }
                    else
                    {
                        //Add person email
                        var emailIds = new List<EmailVerification>();
                        var personEmail = new EmailVerification()
                        {
                            PersonId = Guid.Empty,
                            Address = value.Email,
                            IsVerified = false
                        };

                        if (value.SkipEmailVerification)
                        {
                            personEmail.IsVerified = true;
                        }
                        emailIds.Add(personEmail);

                        Person newPerson = new Person()
                        {
                            Company = value.Company,
                            Department = value.Department,
                            Name = value.Name,
                            EmailVerifications = emailIds
                        };
                        var person = personRepository.Add(newPerson);

                        if (!value.SkipEmailVerification)
                        {
                            if (IsEmailAllowed)
                            {
                                string code = await userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
                                EmailMessage emailMessage = new EmailMessage();
                                emailMessage.Body = SendConfirmationEmail(code, user.Id, passwordString, "en");
                                emailMessage.Subject = "Confirm your account at " + Constants.PRODUCT;
                                await emailSender.SendEmailAsync(emailMessage).ConfigureAwait(false);
                            }
                            else
                            {
                                value.SkipEmailVerification = true;
                                ModelState.AddModelError("Email", "Email is disabled.  Verification email was not sent.");
                            }
                        }

                        //Update the user 
                        if (person != null)
                        {
                            var registeredUser = userManager.FindByNameAsync(user.UserName).Result;
                            registeredUser.PersonId = person.Id.GetValueOrDefault();
                            registeredUser.ForcedPasswordChange = true;
                            await userManager.UpdateAsync(registeredUser).ConfigureAwait(false);

                            //Add person to organization only if you are admin or add it to access request table
                            if (isRequestingUserAdministrator)
                            {
                                OrganizationMember newOrgMember = new OrganizationMember()
                                {
                                    PersonId = person.Id,
                                    OrganizationId = Guid.Parse(organizationId),
                                    IsAutoApprovedByEmailAddress = true,
                                    IsInvited = true
                                };
                                await base.PostEntity(newOrgMember).ConfigureAwait(false);
                            }
                            else {
                                //Add it to access requests
                                AccessRequest accessRequest = new AccessRequest() { 
                                    OrganizationId = Guid.Parse(organizationId),
                                    PersonId = person.Id,
                                    IsAccessRequested = true,
                                    AccessRequestedOn = DateTime.UtcNow
                                };

                                accessRequestManager.AddAccessRequest(accessRequest);
                            }
                        }
                    }
                }
                if (IsEmailAllowed)
                    return Ok();
                else return Ok(ModelState);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Delete organization member 
        /// </summary>
        /// <param name="id">Organization member identifier</param>
        /// <response code="200">Ok, if the organization member with the given id has been soft deleted</response>
        /// <response code="400">Bad request, if the id is not provided or not a proper Guid</response>
        /// <response code="403">Unauthorized access, if the user doesn't have permission to delete the organization member</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the organization member with the given id has been deleted</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesDefaultResponseType]
        [Produces("application/json")]
        public async Task<IActionResult> Delete(string organizationId, string id)
        {
            //Check if the logged in user is member of the organization, do not allow self deletion from organization
            var orgId = new Guid(organizationId);
            var orgMemberId = new Guid(id);

            var orgmem = membershipManager.GetOrganizationMember(orgId, SecurityContext.PersonId)?.Items?.FirstOrDefault();
            if (orgmem == null || (orgmem != null && orgmem.IsAdministrator == null) || (orgmem != null && orgmem.IsAdministrator.HasValue && orgmem.IsAdministrator == false))
            {
                ModelState.AddModelError("Delete", "Remove from organization failed, administrator of an organization can only remove members");
                return BadRequest(ModelState);
            }

            var orgMem = repository.Find(null, p => p.OrganizationId == orgId && p.Id == orgMemberId)?.Items?.FirstOrDefault();
            
            //If member is the logged in user, do not delete
            if (orgMem != null && orgMem.PersonId == SecurityContext.PersonId) {
                ModelState.AddModelError("Delete", "cannot remove from the organization");
                return BadRequest(ModelState);
            }
            return await base.DeleteEntity(id);
        }

        /// <summary>
        /// Updates the partial details of organization members
        /// </summary>
        /// <param name="id">Organization member identifier.</param>
        /// <param name="value">Value to be updated</param>
        /// <response code="200">Ok, if update of organization member is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match.</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial organization member values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id, [FromBody]JsonPatchDocument<OrganizationMember> value)
        {
            return await base.PatchEntity(id, value);
        }
        #region Private methods - Security

        //TODO - To be moved to security manager 
        private async Task<IActionResult> RegisterNewUser(SignUpViewModel signupModel)
        {
            IActionResult result;
            var user = new ApplicationUser() { 
                UserName = signupModel.Email, 
                Email = signupModel.Email
            };

            RandomPassword randomPass = new RandomPassword();
            string passwordString = randomPass.GenerateRandomPassword();
            var loginResult = await userManager.CreateAsync(user, passwordString).ConfigureAwait(false);

            if (!loginResult.Succeeded)
            {
                result = GetErrorResult(loginResult);
            }
            else
            {
                //Add person email
                var emailIds = new List<EmailVerification>();
                var personEmail = new EmailVerification()
                {
                    PersonId = Guid.Empty,
                    Address = signupModel.Email
                };
                emailIds.Add(personEmail);

                Person newPerson = new Person()
                {
                    Company = signupModel.Organization,
                    Department = signupModel.Department,
                    Name = signupModel.Name,
                    EmailVerifications = emailIds
                };
                var person = personRepository.Add(newPerson);

                bool isEmailAllowed = emailSender.IsEmailAllowed();
                if (isEmailAllowed)
                {
                    string code = await userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
                    EmailMessage emailMessage = new EmailMessage();
                    emailMessage.Body = SendConfirmationEmail(code, user.Id, passwordString, "en");
                    emailMessage.Subject = "Confirm your account at " + Constants.PRODUCT;
                    await emailSender.SendEmailAsync(emailMessage).ConfigureAwait(false);
                }
                else
                {
                    ModelState.AddModelError("Email", "Email has been disabled.  Please check email accounts or email settings.");
                    return BadRequest(ModelState);
                }

                //Update the user 
                if (person != null)
                {
                    var registeredUser = userManager.FindByNameAsync(user.UserName).Result;
                    registeredUser.PersonId = (Guid)person.Id;
                    registeredUser.ForcedPasswordChange = true;
                    await userManager.UpdateAsync(registeredUser).ConfigureAwait(false);
                }
                result = Ok();
            }
            return result;
        }

        private IActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
                return StatusCode((int)HttpStatusCode.InternalServerError);

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("Signup", error.Description);
                    }
                }
                return BadRequest(ModelState);
            }

            return null;
        }

        private string SendConfirmationEmail(string code, string userId, string password, string language)
        {
            if (language == "en")
            {

                string confirmationLink = Url.Action("ConfirmEmail",
                                                 "Auth", new
                                                 {
                                                     userid = userId,
                                                     code
                                                 }, protocol: HttpContext.Request.Scheme);

                string emailBody = "";

                using (StreamReader reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Email/accountCreation.html")))
                {
                    emailBody = reader.ReadToEnd();
                    emailBody = emailBody.Replace("^Password^", password, StringComparison.OrdinalIgnoreCase);
                    emailBody = emailBody.Replace("^Confirm^", confirmationLink, StringComparison.OrdinalIgnoreCase);
                }

                return emailBody;
            }
            return "";
        }
        #endregion
    }
}