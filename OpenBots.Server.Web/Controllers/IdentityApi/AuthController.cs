using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using OpenBots.Server.Business;
using OpenBots.Server.Core;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Security;
using OpenBots.Server.Security.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenBots.Server.Web;
using OpenBots.Server.Model;
using Newtonsoft.Json;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Options;
using OpenBots.Server.Web.Extensions;
using OpenBots.Server.ViewModel.Email;
using OpenBots.Server.Model.Membership;

namespace OpenBots.Server.WebAPI.Controllers.IdentityApi
{
    /// <summary>
    /// Controller used for token generation
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ApplicationBaseController
    {
        readonly ApplicationIdentityUserManager userManager;
        readonly SignInManager<ApplicationUser> signInManager;
        readonly IConfiguration configuration;
        readonly ILogger<AuthController> logger;
        readonly IMembershipManager membershipManager;
        readonly IPersonRepository personRepository;
        readonly IEmailManager emailSender;
        readonly IPersonEmailRepository personEmailRepository;
        readonly IEmailVerificationRepository emailVerificationRepository;
        readonly IPasswordPolicyRepository passwordPolicyRepository;
        readonly IOrganizationManager organizationManager;
        readonly IAccessRequestsManager accessRequestManager;
        readonly IAccessRequestRepository accessRequestRepository;
        readonly IOrganizationMemberRepository organizationMemberRepository;
        readonly ITermsConditionsManager termsConditionsManager;
        readonly IAgentRepository agentRepository;
        readonly IAuditLogRepository auditLogRepository;
        readonly WebAppUrlOptions webAppUrlOptions;
        readonly IIPFencingManager iPFencingManager;
        readonly IPFencingOptions iPFencingOptions;
        readonly IIPFencingRepository iPFencingRepository;
        readonly IHttpContextAccessor context;
        readonly IOrganizationSettingRepository organizationSettingRepository;

        /// <summary>
        /// AuthController constructor
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <param name="membershipManager"></param>
        /// <param name="personRepository"></param>
        /// <param name="personEmailRepository"></param>
        /// <param name="emailVerificationRepository"></param>
        /// <param name="emailSender"></param>
        /// <param name="organizationManager"></param>
        /// <param name="organizationSettingRepository"></param>
        /// <param name="iPFencingManager"></param>
        /// <param name="accessRequestManager"></param>
        /// <param name="accessRequestRepository"></param>
        /// <param name="agentRepository"></param>
        /// <param name="auditLogRepository"></param>
        /// <param name="context"></param>
        /// <param name="iPFencingRepository"></param>
        /// <param name="organizationMemberRepository"></param>
        /// <param name="passwordPolicyRepository"></param>
        /// <param name="termsConditionsManager"></param>
        public AuthController(
           ApplicationIdentityUserManager userManager,
           SignInManager<ApplicationUser> signInManager,
           IHttpContextAccessor httpContextAccessor,
           IConfiguration configuration,
           ILogger<AuthController> logger,
           IMembershipManager membershipManager,
           IPersonRepository personRepository,
           IPersonEmailRepository personEmailRepository,
           IEmailVerificationRepository emailVerificationRepository,
           IPasswordPolicyRepository passwordPolicyRepository,
           IEmailManager emailSender,
           IOrganizationManager organizationManager,
           IAccessRequestsManager accessRequestManager,
           IAccessRequestRepository accessRequestRepository,
           IOrganizationMemberRepository organizationMemberRepository,
           IAgentRepository agentRepository,
           ITermsConditionsManager termsConditionsManager,
           IAuditLogRepository auditLogRepository,
           IIPFencingManager iPFencingManager,
           IIPFencingRepository iPFencingRepository,
           IHttpContextAccessor context,
           IOrganizationSettingRepository organizationSettingRepository) : base(httpContextAccessor, userManager, membershipManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.logger = logger;
            this.membershipManager = membershipManager;
            this.personRepository = personRepository;
            this.emailSender = emailSender;
            this.personEmailRepository = personEmailRepository;
            this.emailVerificationRepository = emailVerificationRepository;
            this.passwordPolicyRepository = passwordPolicyRepository;
            this.organizationManager = organizationManager;
            this.accessRequestManager = accessRequestManager;
            this.accessRequestRepository = accessRequestRepository;
            this.organizationMemberRepository = organizationMemberRepository;
            this.termsConditionsManager = termsConditionsManager;
            this.agentRepository = agentRepository;
            this.auditLogRepository = auditLogRepository;
            webAppUrlOptions = configuration.GetSection(WebAppUrlOptions.WebAppUrl).Get<WebAppUrlOptions>();
            this.iPFencingManager = iPFencingManager;
            iPFencingOptions = configuration.GetSection(IPFencingOptions.IPFencing).Get<IPFencingOptions>();
            this.iPFencingRepository = iPFencingRepository;
            this.context = context;
            this.organizationSettingRepository = organizationSettingRepository;
        }

        /// <summary>
        /// Login with username and password
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns>JWT Token</returns>
        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> CreateToken([FromBody] Login loginModel)
        {
            logger.LogInformation(string.Format("Login user : {0}", loginModel.UserName));
            if (ModelState.IsValid)
            {
                var ipCheck = iPFencingOptions.IPFencingCheck;
                if (ipCheck != "Disabled")
                {
                    var ipAddress = HttpContext.Connection.RemoteIpAddress;
                    bool isAllowedRequest = iPFencingManager.IsRequestAllowed(ipAddress);

                    if (!isAllowedRequest)
                    {
                        return Forbid();
                    }
                }

                ApplicationUser user = null;
                //sign in user id
                string signInUser = loginModel.UserName;
                if (RegexUtilities.IsValidEmail(signInUser))
                {
                    //first check if email id exists
                    user = await userManager.FindByEmailAsync(signInUser).ConfigureAwait(true);
                }
                else //no email id, then find by username
                {
                    user = await userManager.FindByNameAsync(signInUser).ConfigureAwait(true);
                }

                if (user == null) return Unauthorized();
                signInUser = user?.UserName;

                var loginResult = await signInManager.PasswordSignInAsync(signInUser, loginModel.Password, isPersistent: false, lockoutOnFailure: false).ConfigureAwait(true);
                if (!loginResult.Succeeded)
                {
                    return Unauthorized();
                }

                Person person = personRepository.Find(null, p => p.Id == user.PersonId)?.Items.FirstOrDefault();
                string authenticationToken = GetToken(user);
                VerifyUserEmailAsync(user);

                var agentId = (Guid?)null;
                if (person.IsAgent)
                {
                    agentId = agentRepository.Find(null, p => p.Name == person.Name)?.Items?.FirstOrDefault()?.Id;
                }

                string startsWith = "";
                int skip = 0;
                int take = 100;
                var personOrgs = membershipManager.Search(user.PersonId, startsWith, skip, take);
                //issue #2791 We will disable the need for User Consent for this release
                bool isUserConsentRequired = false; //VerifyUserAgreementConsentStatus(user.PersonId);
                var pendingAcessOrgs = membershipManager.PendingOrganizationAccess(user.PersonId);
                var newRefreshToken = GenerateRefreshToken();
                var authenticatedUser = new
                {
                    personId = user.PersonId,
                    email = user.Email,
                    userName = user.UserName,
                    token = authenticationToken,
                    refreshToken = newRefreshToken,
                    user.ForcedPasswordChange,
                    isUserConsentRequired,
                    IsJoinOrgRequestPending = (pendingAcessOrgs?.Items?.Count > 0) ? true : false,
                    myOrganizations = personOrgs?.Items,
                    agent = agentId
            };
                //save refresh token
                await userManager.SetAuthenticationTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, "refresh", newRefreshToken).ConfigureAwait(false);
                try
                {
                    if (person.IsAgent == false)
                    {
                        AuditLog auditLog = new AuditLog();
                        auditLog.ChangedFromJson = null;
                        auditLog.ChangedToJson = JsonConvert.SerializeObject(authenticatedUser);
                        auditLog.CreatedBy = user.Email;
                        auditLog.CreatedOn = DateTime.UtcNow;
                        auditLog.Id = Guid.NewGuid();
                        auditLog.IsDeleted = false;
                        auditLog.MethodName = "Login";
                        auditLog.ServiceName = this.ToString();
                        auditLog.Timestamp = new byte[1];
                        auditLog.ParametersJson = "";
                        auditLog.ExceptionJson = "";

                        auditLogRepository.Add(auditLog); //log entry
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("Audit Log", ex.Message);
                    return BadRequest();
                }
                return Ok(authenticatedUser);
            }
            return BadRequest(ModelState);
        }

        private async void VerifyUserEmailAsync(ApplicationUser user)
        {
            //verify email address is confirmed with identity 
            var emailConfirmed = userManager.IsEmailConfirmedAsync(user).Result;
            if (!emailConfirmed)
            {
                string emailVerificationToken = userManager.GenerateEmailConfirmationTokenAsync(user).Result;
                var result = userManager.ConfirmEmailAsync(user, emailVerificationToken).Result;

                if (result.Succeeded)
                { 
                    var emailVerification = emailVerificationRepository.Find(null, p => p.PersonId == user.PersonId && p.IsVerified != true)?.Items?.FirstOrDefault();
                    if (emailVerification != null)
                    {
                        var verifiedEmailAddress = personEmailRepository.Find(null, p => p.Address.Equals(emailVerification.Address, StringComparison.OrdinalIgnoreCase))?.Items?.FirstOrDefault();
                        if (verifiedEmailAddress == null)
                        {
                            var personEmail = new PersonEmail()
                            {
                                EmailVerificationId = emailVerification.Id,
                                IsPrimaryEmail = true,
                                PersonId = emailVerification.PersonId,
                                Address = emailVerification.Address
                            };
                            personEmailRepository.Add(personEmail);
                        }

                        //verification completed
                        emailVerification.IsVerified = true;
                        emailVerificationRepository.Update(emailVerification);
                    }
                }
            }
        }

        /// <summary>
        /// Register new user
        /// </summary>
        /// <param name="signupModel">Signup model</param>
        /// <returns>Ok response</returns>
        [HttpPost]
        [Route("Register")]
        [ProducesResponseType(typeof(ServiceBadRequest), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(SignUpViewModel signupModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (signupModel.CreateNewOrganization)
            {
                var checkOrganization = organizationManager.GetDefaultOrganization();
                if (checkOrganization != null)
                {
                    ModelState.AddModelError("", "Default organization exists, you can not create new organization.");
                    return BadRequest(ModelState);
                }
            }

            EmailVerification emailAddress = emailVerificationRepository.Find(null, p => p.Address.Equals(signupModel.Email, StringComparison.OrdinalIgnoreCase)).Items?.FirstOrDefault();
            if (emailAddress != null)
            {
                organizationMemberRepository.ForceIgnoreSecurity();                
                var member = organizationMemberRepository.Find(null, m => m.PersonId == emailAddress.PersonId)?.Items?.FirstOrDefault();
                organizationMemberRepository.ForceSecurity();

                if (member != null)
                {
                    //already a member of the organization
                    ModelState.AddModelError("Register", "Email address already exists");
                    return BadRequest(ModelState);
                }
                //make a request to join organization
                var oldOrganization = organizationManager.GetDefaultOrganization();
                accessRequestRepository.ForceIgnoreSecurity();
                var existingRequest = accessRequestRepository.Find(null, m => m.PersonId == emailAddress.PersonId)?.Items?.FirstOrDefault();
                accessRequestRepository.ForceSecurity();

                if (oldOrganization != null && existingRequest == null)
                {
                    //update user 
                    if (!IsPasswordValid(signupModel.Password))
                    {
                        ModelState.AddModelError("Password", PasswordRequirementMessage(signupModel.Password));
                        return BadRequest(ModelState);
                    }
                    var existingUser = await userManager.FindByEmailAsync(emailAddress.Address).ConfigureAwait(false);
                    existingUser.Name = signupModel.Name;
                    existingUser.ForcedPasswordChange = false;
                    existingUser.PasswordHash = userManager.PasswordHasher.HashPassword(existingUser, signupModel.Password);

                    var result = await userManager.UpdateAsync(existingUser).ConfigureAwait(true);

                    if (!result.Succeeded)
                    {
                        return GetErrorResult(result);
                    }         

                    Person person = personRepository.Find(null, p => p.Id == emailAddress.PersonId)?.Items?.FirstOrDefault();
                    person.Name = signupModel.Name;
                    person.Department = signupModel.Department;
                    personRepository.Update(person);

                    //create a new access request
                    Model.Membership.AccessRequest accessRequest = new Model.Membership.AccessRequest()
                    {
                        OrganizationId = oldOrganization.Id,
                        PersonId = person.Id,
                        IsAccessRequested = true,
                        AccessRequestedOn = DateTime.UtcNow
                    };

                    accessRequestManager.AddAnonymousAccessRequest(accessRequest);
                    return Ok(new { message = "Access Request has been created for existing user" });
                }
                else
                {
                    ModelState.AddModelError("Register", "Access request already exists");
                    return BadRequest(ModelState);
                }

            }

            if (signupModel.CreateNewOrganization == true && string.IsNullOrWhiteSpace(signupModel.Organization))
            {
                ModelState.AddModelError("", "Organization Name is required");
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser()
            {
                Name = signupModel.Name,
                UserName = signupModel.Email,
                Email = signupModel.Email,
                ForcedPasswordChange = false //set this property to not show password reset secreen for new user
            };

            RandomPassword randomPass = new RandomPassword();
            string passwordString = "";
            bool isPasswordProvided = false;

            if (string.IsNullOrWhiteSpace(signupModel.Password))
            {
                passwordString = randomPass.GenerateRandomPassword();
                isPasswordProvided = false;
            }
            else
            {
                passwordString = signupModel.Password;
                isPasswordProvided = true;
            }
            
            var loginResult = await userManager.CreateAsync(user, passwordString).ConfigureAwait(false);
            if (!loginResult.Succeeded)
            {
                return GetErrorResult(loginResult);
            }
            else
            {
                //add person email
                var emailIds = new List<EmailVerification>();
                var personEmail = new EmailVerification()
                {
                    PersonId = Guid.Empty,
                    Address = signupModel.Email,
                    IsVerified = false
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

                //create new organization if count is zero
                if (signupModel.CreateNewOrganization)
                {
                    Organization value = new Organization();
                    value.Name = signupModel.Organization;
                    value.Description = "System created organization";
                    var newOrganization = organizationManager.AddNewOrganization(value);

                    if (newOrganization != null && newOrganization.Id != null)
                    {
                        OrganizationMember newOrgMember = new OrganizationMember()
                        {
                            PersonId = person.Id,
                            OrganizationId = newOrganization.Id,
                            IsAutoApprovedByEmailAddress = true,
                            IsAdministrator = true
                        };

                        organizationMemberRepository.ForceIgnoreSecurity();
                        organizationMemberRepository.Add(newOrgMember);
                        organizationMemberRepository.ForceSecurity();

                        //update IP fencing allow rule for the current user
                        var ipAddress = context.HttpContext.Connection.RemoteIpAddress;
                        var rule = iPFencingRepository.Find(0, 1).Items?.Where(q => q.IPAddress == ipAddress.ToString()).FirstOrDefault();
                        if (rule != null && rule.OrganizationId == null)
                        {
                            rule.OrganizationId = newOrganization.Id;
                            iPFencingRepository.Update(rule);
                        }
                        else
                        {
                            rule = new IPFencing()
                            {
                                IPAddress = ipAddress.ToString(),
                                OrganizationId = newOrganization.Id,
                                CreatedBy = user.Name,
                                CreatedOn = DateTime.UtcNow,
                                Rule = RuleType.IPv4,
                                Usage = UsageType.Allow
                            };
                            iPFencingRepository.Add(rule);
                        }

                        //add organization setting to deny all users except current one
                        var orgSettings = new OrganizationSetting()
                        {
                            OrganizationId = newOrganization.Id,
                            IPFencingMode = IPFencingMode.AllowMode,
                            DisallowAllExecutions = false,
                            CreatedBy = user.Name,
                            CreatedOn = DateTime.UtcNow
                        };
                        organizationSettingRepository.ForceIgnoreSecurity();
                        organizationSettingRepository.Add(orgSettings);
                        organizationSettingRepository.ForceSecurity();
                    }
                }
                else
                {

                    var oldOrganization = organizationManager.GetDefaultOrganization();
                    if (oldOrganization != null)
                    {
                        //add it to access requests
                        AccessRequest accessRequest = new AccessRequest()
                        {
                            OrganizationId = oldOrganization.Id,
                            PersonId = person.Id,
                            IsAccessRequested = true,
                            AccessRequestedOn = DateTime.UtcNow
                        };

                        accessRequestManager.AddAnonymousAccessRequest(accessRequest);
                    }
                }

                //update the user 
                if (person != null)
                {
                    var registeredUser = userManager.FindByNameAsync(user.UserName).Result;
                    registeredUser.PersonId = (Guid)person.Id;
                    registeredUser.ForcedPasswordChange = true;
                    await userManager.UpdateAsync(registeredUser).ConfigureAwait(false);
                }

                bool IsEmailAllowed = emailSender.IsEmailAllowed();
                if (IsEmailAllowed)
                {
                    string code = await userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
                    EmailMessage emailMessage = new EmailMessage();
                    EmailAddress address = new EmailAddress(user.Name, user.Email);
                    emailMessage.To.Add(address);
                    emailMessage.Body = SendConfirmationEmail(code, user.Id, passwordString, "en");
                    emailMessage.Subject = "Confirm your account at " + Constants.PRODUCT;
                    await emailSender.SendEmailAsync(emailMessage, null, null, "Outgoing").ConfigureAwait(false);
                    return Ok(new { message = "You have successfully registered. A confirmation email has been sent" });
                }
                else
                {
                    return Ok(new { message = "Email is disabled.  Verification email was not sent." });
                }
            }
        }
     
        /// <summary>
        /// Change / Reset with new password
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Ok response</returns>
        [HttpPut]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (applicationUser == null)
                return Unauthorized();
            
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!IsPasswordValid(model.NewPassword))
            {
                ModelState.AddModelError("Password", PasswordRequirementMessage(model.NewPassword));
                return BadRequest(ModelState);
            }

            applicationUser.ForcedPasswordChange = false;
            IdentityResult result = await userManager.ChangePasswordAsync(applicationUser, model.OldPassword, model.NewPassword).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }
            return Ok();
        }

        /// <summary>
        /// Verify user token before resetting the password
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns>Redirect to appropriate url</returns>
        [HttpGet("VerifyUserToken")]
        public async Task<IActionResult> VerifyUserToken(string userId, string code)
        {
            ApplicationUser user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
            if (user == null)
                return Redirect(string.Format("{0}{1}", webAppUrlOptions.Url, webAppUrlOptions.NoUserExists));
            if (userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", code).Result)
            {
                string baseUrl = string.Format(@"{0}{1}", webAppUrlOptions.Url, webAppUrlOptions.Forgotpassword);
                var callbackUrl = string.Format(@"{0}?userid={1}&token={2}", baseUrl, WebUtility.UrlEncode(userId), WebUtility.UrlEncode(code));
                return Redirect(callbackUrl);
            }
            else return Redirect(string.Format("{0}{1}", webAppUrlOptions.Url, webAppUrlOptions.Tokenerror));
        }

        /// <summary>
        /// Reset password
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Ok response</returns>
        [HttpPut]
        [AllowAnonymous]
        [Route("SetPassword")]
        public async Task<IActionResult> SetPassword(ResetPasswordBindingModel model)
        {
            if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.NewPassword) || string.IsNullOrEmpty(model.Token))
            {
                ModelState.AddModelError("", "userId or password or token is missing");
                return BadRequest(ModelState);
            }

            if (!IsPasswordValid(model.NewPassword))
            {
                ModelState.AddModelError("Password", PasswordRequirementMessage(model.NewPassword));
                return BadRequest(ModelState);
            }

            ApplicationUser user = await userManager.FindByIdAsync(model.UserId).ConfigureAwait(false);
            user.ForcedPasswordChange = false;
            var token = WebUtility.UrlDecode(model.Token);
            if (user != null)
            {   
                var result = await userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }
            }

            return Ok();
        }

        /// <summary>
        /// Set new password 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("SetUserPassword")]
        public async Task<IActionResult> SetUserPassword(SetPasswordBindingModel model)
        {
            if (string.IsNullOrEmpty(model.NewPassword))
            {
                ModelState.AddModelError("", "Password missing");
                return BadRequest(ModelState);
            }

            if (!IsPasswordValid(model.NewPassword))
            {
                ModelState.AddModelError("Password", PasswordRequirementMessage(model.NewPassword));
                return BadRequest(ModelState);
            }

            applicationUser.ForcedPasswordChange = false;
            applicationUser.PasswordHash = userManager.PasswordHasher.HashPassword(applicationUser, model.NewPassword);
            var result = await userManager.UpdateAsync(applicationUser).ConfigureAwait(true);
            
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }
            return Ok();
        }

        /// <summary>
        /// Confirm new user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns>Redirect to appropriate url</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                ModelState.AddModelError("ConfirmEmail","UserId / Code missing");
                return BadRequest(ModelState);
            }
            IdentityResult result;
            try
            {
                applicationUser = userManager.FindByIdAsync(userId).Result;
                if (applicationUser == null)
                    return Redirect(string.Format("{0}{1}", webAppUrlOptions.Url, webAppUrlOptions.NoUserExists));
                result = await userManager.ConfirmEmailAsync(applicationUser, code);
            }
            catch (InvalidOperationException ioe)
            {
                //method ConfirmEmailAsync throws exception when the userId is not found
                return ioe.GetActionResult();
            }

            if (result.Succeeded)
            { 
                var emailVerification = emailVerificationRepository.Find(null, p => p.PersonId == applicationUser.PersonId && p.IsVerified != true)?.Items?.FirstOrDefault();
                if (emailVerification != null)
                {
                    var verifiedEmailAddress = personEmailRepository.Find(null, p => p.Address.Equals(emailVerification.Address, StringComparison.OrdinalIgnoreCase))?.Items?.FirstOrDefault();
                    if (verifiedEmailAddress == null)
                    {
                        var personEmail = new PersonEmail()
                        {
                            EmailVerificationId = emailVerification.Id,
                            IsPrimaryEmail = true,
                            PersonId = emailVerification.PersonId,
                            Address = emailVerification.Address
                        };
                        personEmailRepository.Add(personEmail);
                    }

                    //verification completed
                    emailVerification.IsVerified = true;
                    emailVerificationRepository.Update(emailVerification);
                }
                
                return Redirect(string.Format("{0}{1}", webAppUrlOptions.Url, webAppUrlOptions.Login));
            }

            //if we got this far, something failed
            return Redirect(string.Format("{0}{1}", webAppUrlOptions.Url, webAppUrlOptions.Tokenerror));
        }

        /// <summary>
        /// Forgot password using email address
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Ok response</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            ApplicationUser user = await userManager.FindByEmailAsync(model.Email).ConfigureAwait(false);
            if (user != null)
            {
                bool IsEmailAllowed = emailSender.IsEmailAllowed();
                if (IsEmailAllowed)
                {
                    string code = await userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(false);
                    EmailMessage emailMessage = new EmailMessage();
                    EmailAddress address = new EmailAddress(user.Name, user.Email);
                    emailMessage.To.Add(address);
                    emailMessage.Body = SendForgotPasswordEmail(code, user.Id, "en");
                    emailMessage.Subject = string.Format("Reset your password at {0}", Constants.PRODUCT);
                    await emailSender.SendEmailAsync(emailMessage, null, null, "Outgoing").ConfigureAwait(false);
                }
                else
                {
                    ModelState.AddModelError("Email", "Email has been disabled.  Please check email accounts or email settings.");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                ModelState.AddModelError("Email", "Email address does not exist.");
                return BadRequest(ModelState);
            }
            return Ok();
        }

        /// <summary>
        /// Get user info for logged in authenticated user
        /// </summary>
        /// <returns>Ok response with user information</returns>
        [HttpGet]
        [Route("GetUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            if (applicationUser == null)
            {
                return Unauthorized();
            }

            string startsWith = "";
            int skip = 0;
            int take = 100;
            var personOrgs = membershipManager.Search(applicationUser.PersonId, startsWith, skip, take);
            var pendingAcessOrgs = membershipManager.PendingOrganizationAccess(applicationUser.PersonId);
            bool isUserConsentRequired = VerifyUserAgreementConsentStatus(applicationUser.PersonId);
            var authenticatedUser = new
            {
                personId = applicationUser.PersonId,
                email = applicationUser.Email,
                userName = applicationUser.UserName,
                token = Request.Headers["Authorization"].ToString().Replace("bearer ", "", StringComparison.OrdinalIgnoreCase),
                applicationUser.ForcedPasswordChange,
                isUserConsentRequired,
                IsJoinOrgRequestPending = (pendingAcessOrgs?.Items?.Count > 0)? true : false,
                myOrganizations = personOrgs?.Items
            };

            return Ok(authenticatedUser);
        }

        private bool VerifyUserAgreementConsentStatus(Guid personId)
        {
            return false;
        }

        /// <summary>
        /// Resend confirmation email to registered email address 
        /// </summary>
        /// <param name="emailAddress">Email address needed for confirmation</param>
        /// <returns>Ok response</returns>
        [HttpPut]
        [Route("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(string emailAddress) {

            if (applicationUser == null)
                return Unauthorized();

            try
            {
                //resending 
                byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
                byte[] key = applicationUser.PersonId.ToByteArray();
                string token = Convert.ToBase64String(time.Concat(key).ToArray());

                string confirmationLink = Url.Action("ConfirmEmailAddress",
                                                    "Auth", new
                                                    {
                                                        emailAddress,
                                                        token
                                                    }, protocol: HttpContext.Request.Scheme);

                string emailBody = "";

                //using (StreamReader reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Email/confirm-en.html")))
                //{
                //    emailBody = reader.ReadToEnd();
                //    emailBody = emailBody.Replace("^Confirm^", confirmationLink);
                //}

                var templateObj = new EmailTemplateData
                {
                    HrefLink = confirmationLink,
                    Url = AppDomain.CurrentDomain.BaseDirectory,
                    ApiUrl = configuration["WebAppUrl:ApiUrl"],
                    FileName = "Email/confirm-en.html"
                };
                emailBody = EmailTextFormatter.Format(templateObj);


                bool IsEmailAllowed = emailSender.IsEmailAllowed();
                if (IsEmailAllowed)
                {
                    EmailMessage emailMessage = new EmailMessage();
                    EmailAddress address = new EmailAddress(applicationUser?.Name, emailAddress);
                    emailMessage.To.Add(address);
                    emailMessage.Body = emailBody;
                    emailMessage.Subject = "Confirm your email address at " + Constants.PRODUCT;
                    await emailSender.SendEmailAsync(emailMessage, null, null, "Outgoing").ConfigureAwait(false);
                }
                else
                {
                    ModelState.AddModelError("Email", "Email has been disabled.  Please check email accounts or email settings.");
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
            return Ok();
        }

        /// <summary>
        /// Confirm email address
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Redirect to appropriate url</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("ConfirmEmailAddress")]
        public async Task<IActionResult> ConfirmEmailAddress(string emailAddress, string token)
        {
            //to decode the token to get the creation time, person id
            byte[] data = Convert.FromBase64String(token);
            byte[] _time = data.Take(8).ToArray();
            byte[] _key = data.Skip(8).ToArray();

            DateTime when = DateTime.FromBinary(BitConverter.ToInt64(_time, 0));
            Guid personId = new Guid(_key);
            if (when < DateTime.UtcNow.AddHours(-24))
            {
                return Redirect(string.Format("{0}{1}", webAppUrlOptions.Url, webAppUrlOptions.Tokenerror));
            }

            var emailVerification = emailVerificationRepository.Find(null, p => p.PersonId == personId && p.Address.Equals(emailAddress, StringComparison.OrdinalIgnoreCase) && p.IsVerified != true)?.Items?.FirstOrDefault();
            if (emailVerification != null)
            {
                var verifiedEmailAddress = personEmailRepository.Find(null, p => p.Address.Equals(emailVerification.Address, StringComparison.OrdinalIgnoreCase))?.Items?.FirstOrDefault();
                if (verifiedEmailAddress == null)
                {
                    var personEmail = new PersonEmail()
                    {
                        EmailVerificationId = emailVerification.Id,
                        IsPrimaryEmail = false,
                        PersonId = emailVerification.PersonId,
                        Address = emailVerification.Address
                    };
                    personEmailRepository.Add(personEmail);
                }

                //verification completed
                emailVerification.IsVerified = true;
                emailVerificationRepository.Update(emailVerification);
            }
            return Redirect(string.Format("{0}{1}", webAppUrlOptions.Url, webAppUrlOptions.Emailaddressconfirmed));
        }

        /// <summary>
        /// Refresh expired access and old refresh token
        /// </summary>
        /// <param name="model"></param>
        /// <returns>JWT token and refresh token</returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("Refresh")]
        public async Task<IActionResult> Refresh(RefreshModel model)
        {
            var ipCheck = iPFencingOptions.IPFencingCheck;
            if (ipCheck != "Disabled")
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress;
                bool isAllowedRequest = iPFencingManager.IsRequestAllowed(ipAddress);

                if (!isAllowedRequest)
                {
                    return Forbid();
                }
            }

            var principal = GetPrincipalFromExpiredToken(model.Token);
            var username = principal.Identity.Name;
            var savedRefreshToken = await GetRefreshToken(username); //retrieve the refresh token from [AspNetUserTokens] table

            if (savedRefreshToken != model.RefreshToken)
            {
                await signInManager.SignOutAsync().ConfigureAwait(true);
                ModelState.AddModelError("Invalid Token", "Token is no longer valid. Please log back in.");
                return BadRequest(ModelState);
            }

            var newJwtToken = GenerateAccessToken(principal.Claims);
            var newRefreshToken = GenerateRefreshToken();
            await DeleteRefreshToken(username).ConfigureAwait(true);
            await SaveRefreshToken(username, newRefreshToken).ConfigureAwait(true);

            return new ObjectResult(new
            {
                jwt = newJwtToken,
                refreshToken = newRefreshToken
            });
        }

        /// <summary>
        /// Used to get current user's IP Address
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("Ping")]
        public async Task<string> Ping()
        {
            return HttpContext.Connection.RemoteIpAddress.ToString();
        }
        private async Task<string> GetRefreshToken(string username)
        {
            var user = await userManager.FindByNameAsync(username).ConfigureAwait(true);
            var token = await userManager.GetAuthenticationTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, "refresh").ConfigureAwait(true);

            return token;
        }

        private async Task DeleteRefreshToken(string username)
        {
            var user = await userManager.FindByNameAsync(username).ConfigureAwait(true);
            await userManager.RemoveAuthenticationTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, "refresh").ConfigureAwait(true);
        }

        private async Task SaveRefreshToken(string username, string newRefreshToken)
        {
            var user = await userManager.FindByNameAsync(username).ConfigureAwait(true);
            await userManager.SetAuthenticationTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, "refresh", newRefreshToken).ConfigureAwait(true);
        }

        #region - Private Methods

        private string GetToken(IdentityUser user)
        {
            var utcNow = DateTime.UtcNow;
            var claims = new Claim[]
            {
                        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                        new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<String>("Tokens:Key")));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                    signingCredentials: signingCredentials,
                    claims: claims,
                    notBefore: utcNow,
                    expires: utcNow.AddSeconds(this.configuration.GetValue<int>("Tokens:Lifetime")),
                    audience: configuration.GetValue<String>("Tokens:Audience"),
                    issuer: configuration.GetValue<String>("Tokens:Issuer")
                );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var utcNow = DateTime.UtcNow;
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<String>("Tokens:Key")));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                    signingCredentials: signingCredentials,
                    claims: claims,
                    notBefore: utcNow,
                    expires: utcNow.AddSeconds(this.configuration.GetValue<int>("Tokens:Lifetime")),
                    audience: configuration.GetValue<String>("Tokens:Audience"),
                    issuer: configuration.GetValue<String>("Tokens:Issuer")
            );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Tokens:Key"])),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
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
                        if (error.Code.Equals("InvalidToken", StringComparison.OrdinalIgnoreCase)) {
                            ModelState.AddModelError("Invalid Email Token", "Email link expired or has been used");
                        } else ModelState.AddModelError("Signup", error.Description);
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
                                                     code = code
                                                 }, protocol: HttpContext.Request.Scheme);

                string emailBody = "";

                //using (StreamReader reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Email/accountCreation.html")))
                //{
                //    emailBody = reader.ReadToEnd();
                //    emailBody = emailBody.Replace("^Password^", password);
                //    emailBody = emailBody.Replace("^Confirm^", confirmationLink);
                //}

                var templateObj = new EmailTemplateData
                {
                    Password = password,
                    HrefLink = confirmationLink,
                    Url = AppDomain.CurrentDomain.BaseDirectory,
                    ApiUrl = configuration["WebAppUrl:ApiUrl"],
                    FileName = "Email/accountCreation.html"
                };
                emailBody = EmailTextFormatter.Format(templateObj);

                return emailBody;
            }

            return "";
        }

        private string SendForgotPasswordEmail(string code, string userId, string language)
        {
            string confirmationLink = Url.Action("VerifyUserToken",
                                               "Auth", new
                                               {
                                                   userid = userId,
                                                   code = code
                                               }, protocol: HttpContext.Request.Scheme);
            if (language == "en")
            {
                //StreamReader reader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Email/forgotPassword-en.html"));
                //string emailBody = reader.ReadToEnd();
                //emailBody = emailBody.Replace("^resetpassword^", confirmationLink);

                var templateObj = new EmailTemplateData
                {
                    HrefLink = confirmationLink,
                    Url = AppDomain.CurrentDomain.BaseDirectory,
                    ApiUrl = configuration["WebAppUrl:ApiUrl"],
                    FileName = "Email/forgotPassword-en.html"
                };
                string emailBody = EmailTextFormatter.Format(templateObj);

                return emailBody;
            }
            return null;
        }

        private bool IsPasswordValid(string password)
        {
            bool validPassword = true;
            var passwordPolicy = passwordPolicyRepository.Find(0, 0)?.Items?.FirstOrDefault();
            if (passwordPolicy != null)
            {
                PasswordOptions passwordOptions = new PasswordOptions() { 
                    RequiredLength = (int)passwordPolicy.MinimumLength,
                    RequireLowercase = (bool)passwordPolicy.RequireAtleastOneLowercase,
                    RequireNonAlphanumeric = (bool)passwordPolicy.RequireAtleastOneNonAlpha,
                    RequireUppercase = (bool)passwordPolicy.RequireAtleastOneUppercase,
                    RequiredUniqueChars = 0,
                    RequireDigit=(bool)passwordPolicy.RequireAtleastOneNumber
                };
                
                validPassword  = PasswordManager.IsValidPassword(password, passwordOptions);
            }
            return validPassword;
        }

        private string PasswordRequirementMessage(string password)
        {
            var passwordPolicy = passwordPolicyRepository.Find(0, 0)?.Items?.FirstOrDefault();
            if (passwordPolicy != null)
            {
                PasswordOptions passwordOptions = new PasswordOptions()
                {
                    RequiredLength = (int)passwordPolicy.MinimumLength,
                    RequireLowercase = (bool)passwordPolicy.RequireAtleastOneLowercase,
                    RequireNonAlphanumeric = (bool)passwordPolicy.RequireAtleastOneNonAlpha,
                    RequireUppercase = (bool)passwordPolicy.RequireAtleastOneUppercase,
                    RequiredUniqueChars = 0,
                    RequireDigit = (bool)passwordPolicy.RequireAtleastOneNumber
                };

                return PasswordManager.PasswordRequirementMessage(password, passwordOptions);
            }
            return string.Empty;
        }
        #endregion
    }
}