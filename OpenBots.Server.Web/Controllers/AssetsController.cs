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
using OpenBots.Server.ViewModel.ViewModels;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web
{
    /// <summary>
    /// Controller for Assets
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class AssetsController : EntityController<Asset>
    {
        private readonly IProcessManager processManager;
        private readonly IBinaryObjectRepository binaryObjectRepo;
        private readonly IBinaryObjectManager binaryObjectManager;

        /// <summary>
        /// AssetsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="processManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="binaryObjectManager"></param>
        /// <param name="configuration"></param>
        /// <param name="binaryObjectRepo"></param>
        public AssetsController(
            IAssetRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IProcessManager processManager,
            IHttpContextAccessor httpContextAccessor,
            IBinaryObjectManager binaryObjectManager,
            IConfiguration configuration,
            IBinaryObjectRepository binaryObjectRepo) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.processManager = processManager;
            this.binaryObjectRepo = binaryObjectRepo;
            this.binaryObjectManager = binaryObjectManager;
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
        /// <returns>Paginated list of all assets</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<Asset>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<Asset> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
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
        public int? Count(
            [FromQuery(Name = "$filter")] string filter = "")
        {
            return base.Count();
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
        /// Create a new asset entity
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, new asset created and returned</response>
        /// <response code="400">Bad request, when the asset value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created asset details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Asset), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] Asset request)
        {
            try
            {
                var asset = repository.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null))?.Items?.FirstOrDefault();
                if (asset != null)
                {
                    ModelState.AddModelError("Asset", "Asset Name Already Exists");
                    return BadRequest(ModelState);
                }

                var response = await base.PostEntity(request);
                return response;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Create a new binary object and upload asset file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <response code="200">Ok, asset updated and returned</response>
        /// <response code="400">Bad request, when the asset value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly updated asset details</returns>
        [HttpPost("{id}/upload")]
        [ProducesResponseType(typeof(Asset), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post(string id, [FromForm] IFormFile file)
        {
            try
            {
                if (file == null)
                {
                    ModelState.AddModelError("Save", "No data passed");
                    return BadRequest(ModelState);
                }

                long size = file.Length;
                if (size <= 0)
                {
                    ModelState.AddModelError("Asset Upload", "No asset uploaded");
                    return BadRequest(ModelState);
                }

                var asset = repository.GetOne(Guid.Parse(id));
                string organizationId = binaryObjectManager.GetOrganizationId();
                string apiComponent = "AssetAPI";

                BinaryObject binaryObject = new BinaryObject();
                binaryObject.Name = file.FileName;
                binaryObject.Folder = apiComponent;
                binaryObject.CreatedOn = DateTime.UtcNow;
                binaryObject.CreatedBy = applicationUser?.UserName;
                binaryObject.CorrelationEntityId = asset.Id;
                binaryObjectRepo.Add(binaryObject);

                string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObject.Id.ToString());

                var existingbinary = binaryObjectRepo.Find(null, x => x.Folder?.ToLower(null) == binaryObject.Folder.ToLower(null) && x.Name.ToLower(null) == file?.FileName?.ToLower(null) && x.Id != binaryObject.Id)?.Items?.FirstOrDefault();
                if (existingbinary != null)
                {
                    ModelState.AddModelError("BinaryObject", "Same file name already exists in the given folder");
                    return BadRequest(ModelState);
                }
                
                binaryObjectManager.Upload(file, organizationId, apiComponent, binaryObject.Id.ToString());
                binaryObjectManager.SaveEntity(file, filePath, binaryObject, apiComponent, organizationId);

                asset.BinaryObjectID = binaryObject.Id;
                repository.Update(asset);

                return Ok(asset);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Asset", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Export/download an asset file
        /// </summary>
        /// <param name="id"></param>
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
        public async Task<IActionResult> ExportAsset(string id)
        {
            try
            {
                Guid assetId;
                Guid.TryParse(id, out assetId);

                Asset asset = repository.GetOne(assetId);

                if (asset == null || asset.BinaryObjectID == null || asset.BinaryObjectID == Guid.Empty)
                {
                    ModelState.AddModelError("Asset Export", "No asset or asset file found");
                    return BadRequest(ModelState);
                }

                var fileObject = processManager.Export(asset.BinaryObjectID.ToString());
                var file = File(fileObject?.Result?.BlobStream, fileObject?.Result?.ContentType, fileObject?.Result?.Name);
                return file;
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
                Guid entityId = new Guid(id);

                var existingAsset = repository.GetOne(entityId);
                if (existingAsset == null) return NotFound();

                var asset = repository.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null))?.Items?.FirstOrDefault();
                if (asset != null && asset.Id != entityId)
                {
                    ModelState.AddModelError("Asset", "Asset Name Already Exists");
                    return BadRequest(ModelState);
                }

                existingAsset.Name = request.Name;
                existingAsset.Type = request.Type;
                existingAsset.TextValue = request.TextValue;
                existingAsset.NumberValue = request.NumberValue;
                existingAsset.JsonValue = request.JsonValue;

                return await base.PutEntity(id, existingAsset);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Asset", ex.Message);
                return BadRequest(ModelState);
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
                Guid entityId = new Guid(id);
                var existingAsset = repository.GetOne(entityId);
                if (existingAsset == null) return NotFound();

                string binaryObjectId = existingAsset.BinaryObjectID.ToString();
                var binaryObject = binaryObjectRepo.GetOne(Guid.Parse(binaryObjectId));

                string organizationId = binaryObject.OrganizationId.ToString();
                if (!string.IsNullOrEmpty(organizationId))
                    organizationId = processManager.GetOrganizationId().ToString();

                if (request.Name != null)
                {
                    var asset = repository.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null))?.Items?.FirstOrDefault();
                    if (asset != null && asset.Id != entityId)
                    {
                        ModelState.AddModelError("Asset", "Asset Name Already Exists");
                        return BadRequest(ModelState);
                    }
                }

                if (request == null)
                {
                    ModelState.AddModelError("Save", "No data passed");
                }

                long size = request.file == null ? 0 : request.file.Length;

                try
                {
                    if (!string.IsNullOrEmpty(request.Name))
                        existingAsset.Name = request.Name;
                    else existingAsset.Name = existingAsset.Name;

                    if (!string.IsNullOrEmpty(request.Type))
                        existingAsset.Type = request.Type;
                    else existingAsset.Type = existingAsset.Type;

                    existingAsset.TextValue = request.TextValue;
                    existingAsset.NumberValue = request.NumberValue;
                    existingAsset.JsonValue = request.JsonValue;

                    if (existingAsset.BinaryObjectID != Guid.Empty && size > 0)
                    {
                        //Update Asset file in OpenBots.Server.Web using relative directory
                        string apiComponent = "AssetAPI";
                        await processManager.Update(existingAsset.BinaryObjectID.Value, request.file, organizationId, apiComponent, request.file.FileName);
                    }

                    //Update Asset entity
                    await base.PutEntity(id, existingAsset);
                    return Ok(existingAsset);
                }
                catch (Exception ex)
                {
                    return ex.GetActionResult();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Asset", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Deletes an asset with a specified id
        /// </summary>
        /// <param name="id">Asset id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when asset is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Ba request, if asset id is null or empty Guid</response>
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
                var asset = repository.GetOne(Guid.Parse(id));

                if (asset != null)
                {
                    Guid? binaryObjectId = asset.BinaryObjectID;

                    if (!string.IsNullOrEmpty(binaryObjectId.ToString()))
                        binaryObjectRepo.SoftDelete((Guid)binaryObjectId);

                    return await base.DeleteEntity(id);
                }
                else
                {
                    ModelState.AddModelError("Asset", "Asset cannot be found or does not exist.");
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Asset", ex.Message);
                return BadRequest(ModelState);
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
            Guid entityId = new Guid(id);
            for (int i = 0; i < request.Operations.Count; i++)
            {
                if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/name")
                {
                    var asset = repository.Find(null, d => d.Name.ToLower(null) == request.Operations[i].value.ToString().ToLower(null) && d.Id != entityId)?.Items?.FirstOrDefault();
                    if (asset != null)
                    {
                        ModelState.AddModelError("Asset", "Asset Name Already Exists");
                        return BadRequest(ModelState);
                    }
                }
            }
            return await base.PatchEntity(id, request);
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
            Guid entityId = new Guid(id);
            var request = repository.GetOne(entityId);
            if (request == null) return NotFound();

            if (request.Type.ToLower() != "number")
            {
                ModelState.AddModelError("Asset", "Asset is not a Number type");
                return BadRequest(ModelState);
            }

            request.NumberValue = request.NumberValue + 1;
            return await base.PutEntity(id, request);
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
            Guid entityId = new Guid(id);
            var request = repository.GetOne(entityId);
            if (request == null) return NotFound();
            
            if (request.Type.ToLower() != "number")
            {
                ModelState.AddModelError("Asset", "Asset is not a Number type");
                return BadRequest(ModelState);
            }

            request.NumberValue = request.NumberValue - 1;
            return await base.PutEntity(id, request);
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
            Guid entityId = new Guid(id);
            var request = repository.GetOne(entityId);
            if (request == null) return NotFound();
            
            if (request.Type.ToLower() != "number")
            {
                ModelState.AddModelError("Asset", "Asset is not a Number type");
                return BadRequest(ModelState);
            }

            request.NumberValue = request.NumberValue + value;
            return await base.PutEntity(id, request);
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
            Guid entityId = new Guid(id);
            var request = repository.GetOne(entityId);
            if (request == null) return NotFound();
           
            if (request.Type.ToLower() != "number")
            {
                ModelState.AddModelError("Asset", "Asset is not a Number type");
                return BadRequest(ModelState);
            }

            request.NumberValue = request.NumberValue - value;
            return await base.PutEntity(id, request);
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
            Guid entityId = new Guid(id);
            var request = repository.GetOne(entityId);
            if (request == null) return NotFound();
            
            if (request.Type.ToLower() != "text")
            {
                ModelState.AddModelError("Asset", "Asset is not a Text type");
                return BadRequest(ModelState);
            }

            request.TextValue = string.Concat(request.TextValue, " ", value);
            return await base.PutEntity(id, request);
        }
    }
}
