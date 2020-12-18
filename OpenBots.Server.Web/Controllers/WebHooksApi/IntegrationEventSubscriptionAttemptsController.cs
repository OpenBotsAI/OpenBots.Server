using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Webhooks;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers.WebHooksApi
{
    /// <summary>
    /// ReadOnlyController for IntegrationEventSubscriptionAttempts events
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class IntegrationEventSubscriptionAttemptsController : ReadOnlyEntityController<IntegrationEventSubscriptionAttempt>
    {
        private readonly IIntegrationEventSubscriptionAttemptRepository repository;
        private readonly IIntegrationEventSubscriptionAttemptManager attemptManager;
        public IntegrationEventSubscriptionAttemptsController(
            IIntegrationEventSubscriptionAttemptRepository repository,
            IIntegrationEventSubscriptionAttemptManager attemptManager,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.repository = repository;
            this.attemptManager = attemptManager;
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
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides a view model list of all SubscriptionAttempts
        /// </summary>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <param name="orderBy"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a paginated list of all SubscriptionAttempts</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all SubscriptionAttempts</returns>
        [HttpGet("view")]
        [ProducesResponseType(typeof(PaginatedList<SubscriptionAttemptViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<SubscriptionAttemptViewModel> View(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            ODataHelper<SubscriptionAttemptViewModel> oData = new ODataHelper<SubscriptionAttemptViewModel>();

            string queryString = "";

            if (HttpContext != null
                && HttpContext.Request != null
                && HttpContext.Request.QueryString != null
                && HttpContext.Request.QueryString.HasValue)
                queryString = HttpContext.Request.QueryString.Value;

            oData.Parse(queryString);
            Guid parentguid = Guid.Empty;
            var newNode = oData.ParseOrderByQuery(queryString);
            if (newNode == null)
                newNode = new OrderByNode<SubscriptionAttemptViewModel>();

            Predicate<SubscriptionAttemptViewModel> predicate = null;
            if (oData != null && oData.Filter != null)
                predicate = new Predicate<SubscriptionAttemptViewModel>(oData.Filter);
            int take = (oData?.Top == null || oData?.Top == 0) ? 100 : oData.Top;

            return repository.FindAllView(predicate, newNode.PropertyName, newNode.Direction, oData.Skip, take);
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

        /// <summary>
        /// Provides a SubscriptionAttempt's view model details for a particular SubscriptionAttempt id
        /// </summary>
        /// <param name="id">SubscriptionAttempt id</param>
        /// <response code="200">Ok, if a SubscriptionAttempt exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if SubscriptionAttempt id is not in the proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no SubscriptionAttempt exists for the given SubscriptionAttempt id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>SubscriptionAttempt view model details for the given id</returns>
        [HttpGet("view/{id}")]
        [ProducesResponseType(typeof(SubscriptionAttemptViewModel), StatusCodes.Status200OK)]
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
                IActionResult actionResult = await base.GetEntity<SubscriptionAttemptViewModel>(id);
                OkObjectResult okResult = actionResult as OkObjectResult;

                if (okResult != null)
                {
                    SubscriptionAttemptViewModel view = okResult.Value as SubscriptionAttemptViewModel;
                    view = attemptManager.GetAttemptView(view);
                }

                return actionResult;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}
