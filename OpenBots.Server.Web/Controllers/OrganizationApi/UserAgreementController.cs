using System;
using System.Threading.Tasks;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Model.Attributes;

namespace OpenBots.Server.WebAPI.Controllers.OrganizationApi
{
    /// <summary>
    /// Controller for User Agreements
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class UserAgreementController : ReadOnlyEntityController<UserAgreement>
    {
        private readonly IUserAgreementRepository _repository;
        private readonly IMembershipManager _membershipManager;
        private readonly IPersonRepository _personRepository;
        private readonly IPersonEmailRepository _personEmailRepository;
        private readonly IEmailVerificationRepository _emailVerificationRepository;
        private readonly ITermsConditionsManager _termsConditionsManager;

        /// <summary>
        /// UserAgreementController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        /// <param name="membershipManager"></param>
        /// <param name="personRepository"></param>
        /// <param name="personEmailRepository"></param>
        /// <param name="emailVerificationRepository"></param>
        /// <param name="termsConditionsManager"></param>
        public UserAgreementController(
            IUserAgreementRepository repository,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IMembershipManager membershipManager,
            IPersonRepository personRepository,
            IPersonEmailRepository personEmailRepository,
            IEmailVerificationRepository emailVerificationRepository,
            ITermsConditionsManager termsConditionsManager,
            IConfiguration configuration) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _repository = repository;
            _membershipManager = membershipManager;
            _personRepository = personRepository;
            _personEmailRepository = personEmailRepository;
            _emailVerificationRepository = emailVerificationRepository;
            _termsConditionsManager = termsConditionsManager;

            _repository.SetContext(SecurityContext);
            _membershipManager.SetContext(SecurityContext);
            _personRepository.SetContext(SecurityContext);
            _personEmailRepository.SetContext(SecurityContext);
            _emailVerificationRepository.SetContext(SecurityContext);
            _termsConditionsManager.SetContext(SecurityContext);
        }

        /// <summary>
        /// Get latest terms and conditions for user consent
        /// </summary>
        /// <response code="200">Ok, latest terms and conditions for user consent</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>        
        /// <response code="422">Unprocessable entity</response>
        /// <returns>User agreement</returns>
        [HttpGet]
        [ProducesResponseType(typeof(UserAgreement), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get()
        {
            try
            {
                var response = await _termsConditionsManager.GetUserAgreement().ConfigureAwait(false);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Check that the user consent has been recorded for logged user on the latest user agreement and IsAccepted property is True and has not expired
        /// If there is no record of acceptance for the latest version of the Terms and Conditions, then response will be false, which will force user consent before proceeding
        /// </summary>
        /// <response code="200">Ok, true or false</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response> 
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with boolean value of true or false</returns>
        [HttpGet("{id}/HasAcceptedAndNotExpired")]
        [ProducesResponseType(typeof(UserAgreement), StatusCodes.Status200OK)]
        [Produces("application/json")]
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
                var response = await _termsConditionsManager.IsAccepted(entityId, SecurityContext.PersonId).ConfigureAwait(false);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}
