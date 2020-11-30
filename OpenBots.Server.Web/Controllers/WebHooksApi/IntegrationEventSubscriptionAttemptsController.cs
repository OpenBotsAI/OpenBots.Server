using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.WebHooks;
using OpenBots.Server.Security;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers.WebHooksApi
{
    /// <summary>
    /// Controller for IntegrationEventSubscriptionAttempts events
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class IntegrationEventSubscriptionAttemptsController : ReadOnlyEntityController<IntegrationEventSubscriptionAttempt>
    {
        public IntegrationEventSubscriptionAttemptsController(
            IIntegrationEventSubscriptionAttemptRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
        }

        /// <summary>
        /// Provides a list of all IntegrationEventSubscriptionAttempts
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of all IntegrationEventSubscriptionAttempts</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>        
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all  IntegrationEventSubscriptionAttempt</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<IntegrationEventSubscriptionAttempt>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<IntegrationEventSubscriptionAttempt> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides an IntegrationEventSubscriptionAttempt's details for a particular id
        /// </summary>
        /// <param name="id">IntegrationEventSubscriptionAttempt id</param>
        /// <response code="200">Ok, if an IntegrationEventSubscriptionAttempt exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if IntegrationEventSubscriptionAttempt id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no IntegrationEventSubscriptionAttempt exists for the given id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>IntegrationEventSubscriptionAttempt details for the given id</returns>
        [HttpGet("{id}", Name = "GetIntegrationEventSubscriptionAttempt")]
        [ProducesResponseType(typeof(IntegrationEventSubscriptionAttempt), StatusCodes.Status200OK)]
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
    }
}
