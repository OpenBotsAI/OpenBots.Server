using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.ViewModels;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenBots.Server.Web
{
    /// <summary>
    /// Controller for Assets
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class AssetsController : EntityController<Asset>
    {
        private readonly IWebhookPublisher _webhookPublisher;
        private readonly IAssetManager _manager;

        /// <summary>
        /// AssetsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        /// <param name="webhookPublisher"></param>
        /// <param name="manager"></param>
        public AssetsController(
            IAssetRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IWebhookPublisher webhookPublisher,
            IAssetManager manager) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _webhookPublisher = webhookPublisher;
            _manager = manager;

            _manager.SetContext(SecurityContext);
        }

        /// <summary>
        /// Provides a list of all assets
        /// </summary>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <response code="200">OK,a Paginated list of all Assets</response>
        /// <response code="400">BadRequest</response>
        /// <response code="403">Forbidden,unauthorized access</response> 
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all Assets</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<Asset>), StatusCodes.Status200OK)]
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
        /// Provides a view model list of all Assets and corresponding Agents and Files
        /// </summary>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <param name="orderBy"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a paginated list of all Assets</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all Assets</returns>
        [HttpGet("view")]
        [ProducesResponseType(typeof(PaginatedList<AllAssetsViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> View(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            try
            {
                ODataHelper<AllAssetsViewModel> oDataHelper = new ODataHelper<AllAssetsViewModel>();
                var oData = oDataHelper.GetOData(HttpContext, oDataHelper);

                return Ok(_manager.GetAutomationsAndAutomationVersions(oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take));
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides a count of assets 
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, total count of assets</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>  
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Total count of assets</returns>
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
        /// Provides an asset's details for a particular asset id
        /// </summary>
        /// <param name="id">Asset id</param>
        /// <response code="200">Ok, if an asset exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if asset id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no asset exists for the given asset id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Asset details for the given id</returns>
        [HttpGet("{id}", Name = "GetAsset")]
        [ProducesResponseType(typeof(Asset), StatusCodes.Status200OK)]
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
        /// Provides an asset's details for a particular asset name
        /// </summary>
        /// <remarks>
        /// If the requesting user is an Agent with an existing Asset, then that value will be returned
        /// </remarks>
        /// <param name="assetName">Asset name</param>
        /// <param name="assetType">Asset type</param>
        /// <response code="200">Ok, if an Asset exists with the given name</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no Asset exists for the given Asset name</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Asset details for the given name</returns>
        [HttpGet("GetAssetByName/{assetName}")]
        [ProducesResponseType(typeof(Asset), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetAssetByName(string assetName, [FromQuery] string assetType)
        {
            try
            {
                return Ok(_manager.GetMatchingAsset(assetName,assetType));
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Create a new asset entity
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, new asset created and returned</response>
        /// <response code="400">Bad request, when the asset value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity, when a duplicate record is being entered</response>
        /// <returns>Newly created asset details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(GlobalAssetViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromForm] GlobalAssetViewModel request)
        {
            try
            {
                Asset asset = new Asset();
                asset = request.Map(request);
                asset = _manager.CreateAsset(asset, request.File, request.DriveName);

                var response = await base.PostEntity(asset);
                await _webhookPublisher.PublishAsync("Assets.NewAssetCreated", asset.Id.ToString(), asset.Name).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new agent asset if a global asset exists for the given name
        /// </summary>
        /// <param name="request">New file to update Asset</param>
        /// <response code="200">Ok, asset created and returned</response>
        /// <response code="400">Bad request, when the asset value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity, when a duplicate record is being entered</response>
        /// <returns>Newly created Asset details</returns>
        [HttpPost("AddAgentAsset")]
        [ProducesResponseType(typeof(Asset), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> AddAgentAsset([FromForm] AgentAssetViewModel request)
        {
            try
            {
                Asset agentAsset = _manager.CreateAgentAsset(request);

                var response = await base.PostEntity(agentAsset);
                await _webhookPublisher.PublishAsync("Assets.NewAssetCreated", agentAsset.Id.ToString(), agentAsset.Name).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Export/download an asset file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="driveName"></param>
        /// <response code="200">Ok if an asset file exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if asset id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no asset file exists for the given asset id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Downloaded asset file</returns>
        [HttpGet("{id}/Export", Name = "ExportAsset")]
        [ProducesResponseType(typeof(MemoryStream), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> ExportAsset(string id, string driveName = null)
        {
            try
            {
                var fileObject = _manager.Export(id, driveName);
                return File(fileObject.Result?.Content, fileObject.Result?.ContentType, fileObject.Result?.Name);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates an asset 
        /// </summary>
        /// <remarks>
        /// Provides an action to update an asset, when asset id and the new details of asset are given
        /// </remarks>
        /// <param name="id">Asset id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Asset details to be updated</param>
        /// <response code="200">Ok, if the asset details for the given asset id have been updated</response>
        /// <response code="400">Bad request, if the asset id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
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
        public async Task<IActionResult> Put(string id, [FromBody] Asset request)
        {
            try
            {
                var existingAsset = _manager.UpdateAsset(id, request);

                _manager.AssetNameAvailability(existingAsset);

                await _webhookPublisher.PublishAsync("Assets.AssetUpdated", existingAsset.Id.ToString(), existingAsset.Name).ConfigureAwait(false);
                return await base.PutEntity(id, existingAsset);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates an asset with file 
        /// </summary>
        /// <remarks>
        /// Provides an action to update an asset with file, when asset id and the new details of asset are given
        /// </remarks>
        /// <param name="id">Asset id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">New file to update Asset</param>
        /// <response code="200">Ok, if the asset details for the given asset id have been updated</response>
        /// <response code="400">Bad request, if the asset id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated asset value</returns>
        [HttpPut("{id}/Update")]
        [ProducesResponseType(typeof(Asset), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromForm] UpdateAssetViewModel request)
        {
            try
            {
                var existingAsset = _manager.UpdateAssetFile(id, request);

                //update asset entity
                await _webhookPublisher.PublishAsync("Assets.AssetUpdated", existingAsset.Id.ToString(), existingAsset.Name).ConfigureAwait(false);
                await base.PutEntity(id, existingAsset);

                return Ok(existingAsset);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Deletes an asset with a specified id
        /// </summary>
        /// <param name="id">Asset id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when asset is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if asset id is null or empty Guid</response>
        /// <response code="403">Forbidden</response>
        /// <returns>Ok response</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete(string id, string driveName = null)
        {
            try
            {
                var asset = _manager.DeleteAsset(id, driveName);

                await _webhookPublisher.PublishAsync("Assets.AssetDeleted", asset.Id.ToString(), asset.Name).ConfigureAwait(false);
                return await base.DeleteEntity(id);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates partial details of asset
        /// </summary>
        /// <param name="id">Asset identifier</param>
        /// <param name="request">Value of the asset to be updated</param>
        /// <response code="200">Ok, if update of asset is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial asset values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<Asset> request)
        {
            try
            {
                var asset = _manager.PatchAsset(id, request);
                await _webhookPublisher.PublishAsync("Assets.AssetUpdated", asset.Id.ToString(), asset.Name).ConfigureAwait(false);
                return await base.PatchEntity(id, request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Increment the number value of an asset
        /// </summary>
        /// <param name="id">Asset identifier</param>
        /// <response code="200">Ok, if update of asset is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with updated asset value</returns>
        [HttpPut("{id}/Increment")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Increment(string id)
        {
            try
            {
                var asset = _manager.Increment(id);
                await _webhookPublisher.PublishAsync("Assets.AssetUpdated", asset.Id.ToString(), asset.Name).ConfigureAwait(false);
                return await base.PutEntity(id, asset);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Decrement the number value of asset
        /// </summary>
        /// <param name="id">Asset identifier</param>
        /// <response code="200">Ok, if update of asset is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with updated asset value</returns>
        [HttpPut("{id}/Decrement")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Decrement(string id)
        {
            try
            {
                var asset = _manager.Decrement(id);
                await _webhookPublisher.PublishAsync("Assets.AssetUpdated", asset.Id.ToString(), asset.Name).ConfigureAwait(false);
                return await base.PutEntity(id, asset);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Add the number value of asset
        /// </summary>
        /// <param name="id">Asset identifier</param>
        /// <param name="value">Value of the asset to be updated</param>
        /// <response code="200">Ok, if update of asset is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match.</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with updated asset value</returns>
        [HttpPut("{id}/Add")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Add(string id, int value)
        {
            try
            {
                var asset = _manager.Add(id, value);
                await _webhookPublisher.PublishAsync("Assets.AssetUpdated", asset.Id.ToString(), asset.Name).ConfigureAwait(false);
                return await base.PutEntity(id, asset);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Subtract the number value of asset
        /// </summary>
        /// <param name="id">Asset identifier</param>
        /// <param name="value">Value of the asset to be updated</param>
        /// <response code="200">Ok, if update of asset is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with updated asset value</returns>
        [HttpPut("{id}/Subtract")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Subtract(string id, int value)
        {
            try
            {
                var asset = _manager.Subtract(id, value);
                await _webhookPublisher.PublishAsync("Assets.AssetUpdated", asset.Id.ToString(), asset.Name).ConfigureAwait(false);
                return await base.PutEntity(id, asset);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Append the text value of asset
        /// </summary>
        /// <param name="id">Asset identifier</param>
        /// <param name="value">Value of the asset to be updated</param>
        /// <response code="200">Ok, if update of asset is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response with updated asset value</returns>
        [HttpPut("{id}/Append")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Append(string id, string value)
        {
            try
            {
                var asset = _manager.Append(id, value);
                await _webhookPublisher.PublishAsync("Assets.AssetUpdated", asset.Id.ToString(), asset.Name).ConfigureAwait(false);
                return await base.PutEntity(id, asset);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}
