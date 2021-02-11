﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement.Mvc;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Options;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for binary object files
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [FeatureGate(MyFeatureFlags.Files)]
    public class BinaryObjectsController : EntityController<BinaryObject>
    {
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IWebhookPublisher _webhookPublisher;

        /// <summary>
        /// BinaryObjectsController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="binaryObjectManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="webhookPublisher"></param>
        public BinaryObjectsController(
            IBinaryObjectRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IBinaryObjectManager binaryObjectManager,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IWebhookPublisher webhookPublisher) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _binaryObjectManager = binaryObjectManager;
            _binaryObjectManager.SetContext(SecurityContext);
            _webhookPublisher = webhookPublisher;
        }

        /// <summary>
        /// Provides a list of all binary objects
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a paginated list of all binary objects</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response> 
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all binary objects</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<BinaryObjectViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get(
            [FromQuery(Name = "$filter")] string filter = "")
        {
            try
            {
                return Ok(base.GetMany<BinaryObjectViewModel>());
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Gets count of BinaryObjects in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, count of binary objects</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all binary objects</returns>
        [HttpGet("count")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetCount(
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
        /// Provides binary object details for a particular binary object id
        /// </summary>
        /// <param name="id">Binary object id</param>
        /// <response code="200">Ok, if a binary object exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if binary object id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no binary object exists for the given binary object id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Binary object details</returns>
        [HttpGet("{id}", Name = "GetBinaryObject")]
        [ProducesResponseType(typeof(PaginatedList<BinaryObject>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetBinaryObject(string id)
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
        /// Creates binary object entity in database
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, new binary object entity created and returned</response>
        /// <response code="400">Bad request, when the binary object value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created binary object details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(BinaryObject), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] BinaryObject request)
        {
            try
            {
                var response = await base.PostEntity(request);
                await _webhookPublisher.PublishAsync("Files.NewFileCreated", request.Id.ToString(), request.Name).ConfigureAwait(false);
                return response;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Creates binary object file and updates entity in database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <response code="200">Ok, new binary object entity created and returned</response>
        /// <response code="400">Bad request, when the binary object value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Updated binary object details</returns>
        [HttpPost("{id}/upload")]
        [ProducesResponseType(typeof(BinaryObject), StatusCodes.Status200OK)]
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
                    ModelState.AddModelError("Save", "No file uploaded");
                    return BadRequest(ModelState);
                }

                long size = file.Length;
                if (size <= 0)
                {
                    ModelState.AddModelError("Automation Upload", $"File size of file {file.FileName} cannot be 0");
                    return BadRequest(ModelState);
                }

                var existingBinaryObject = repository.GetOne(Guid.Parse(id));
                if (existingBinaryObject == null) return NotFound();
                string organizationId = _binaryObjectManager.GetOrganizationId();
                string apiComponent = "BinaryObjectAPI";

                if (string.IsNullOrEmpty(existingBinaryObject.Folder))
                    existingBinaryObject.Folder = apiComponent;

                //find relative directory where binary object is being saved
                string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, existingBinaryObject.Id.ToString());
                if (filePath != null)
                {
                    var existingbinary = repository.Find(null, x => x.Folder?.ToLower(null) == existingBinaryObject.Folder.ToLower(null) && x.Name.ToLower(null) == file?.FileName?.ToLower(null) && x.Id != Guid.Parse(id))?.Items?.FirstOrDefault();
                    if (existingbinary != null)
                    {
                        ModelState.AddModelError("BinaryObject", "Same file name already exists in the given folder");
                        return BadRequest(ModelState);
                    }
                    else
                    {
                        _binaryObjectManager.Upload(file, organizationId, apiComponent, existingBinaryObject.Id.ToString());
                        _binaryObjectManager.SaveEntity(file, filePath, existingBinaryObject, apiComponent, organizationId);
                        repository.Update(existingBinaryObject);
                    }
                }
                await _webhookPublisher.PublishAsync("Files.FileUpdated", existingBinaryObject.Id.ToString(), existingBinaryObject.Name).ConfigureAwait(false);
                return Ok(existingBinaryObject);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Export/Download a binary object
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if a binary object exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if binary object id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no binary object exists for the given binary object id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Binary object file converted back to original file format</returns>
        [HttpGet("{id}/download")]
        [ProducesResponseType(typeof(MemoryStream), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Download(string id)
        {
            try
            {
                Guid binaryObjectId;
                Guid.TryParse(id, out binaryObjectId);

                BinaryObject binaryObject = repository.GetOne(binaryObjectId);

                if (binaryObject == null || binaryObjectId == null || binaryObjectId == Guid.Empty)
                {
                    ModelState.AddModelError("Binary Object Export", "No binary object or binary object file found");
                    return BadRequest(ModelState);
                }

                var fileObject = _binaryObjectManager.Download(binaryObjectId.ToString());

                return File(fileObject?.Result?.BlobStream, fileObject?.Result?.ContentType, fileObject?.Result?.Name);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates a binary object 
        /// </summary>
        /// <remarks>
        /// Provides an action to update a binary object, when binary object id and the new details of binary object are given
        /// </remarks>
        /// <param name="id">Binary object id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Binary object details to be updated</param>
        /// <response code="200">Ok, if the binary object details for the given binary object id has been updated</response>
        /// <response code="400">Bad request, if the binary object id is null or ids don't match</response>
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
        public async Task<IActionResult> Put(string id, [FromBody] BinaryObject request)
        {
            try
            {
                Guid entityId = new Guid(id);

                if (string.IsNullOrEmpty(request.Folder))
                    request.Folder = request.CorrelationEntity;

                var existingbinary = repository.Find(null, x => x.Name?.ToLower(null) == request.Name?.ToLower(null) && x.Folder?.ToLower(null) == request.Folder?.ToLower(null) && x.Id != entityId)?.Items?.FirstOrDefault();
                if (existingbinary != null)
                {
                    ModelState.AddModelError("BinaryObject", "Same file name already exists in the given folder");
                    return BadRequest(ModelState);
                }

                var existingBinaryObject = repository.GetOne(entityId);
                if (existingBinaryObject == null) return NotFound();

                existingBinaryObject.Name = request.Name ?? existingBinaryObject.Name;
                existingBinaryObject.Folder = request.Folder;

                await _webhookPublisher.PublishAsync("Files.FileUpdated", existingBinaryObject.Id.ToString(), existingBinaryObject.Name).ConfigureAwait(false);
                return await base.PutEntity(id, existingBinaryObject);
                                     
            }
            catch (Exception ex)     
            {
                return ex.GetActionResult();
            }  
        }

        /// <summary>
        /// Updates a binary object with file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <response code="200">Ok, updated binary object details</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Updated binary object details</returns>
        [HttpPut("{id}/update")]
        [ProducesResponseType(typeof(BinaryObject), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Update(string id, [FromForm] BinaryObjectViewModel request)
        {
            try
            {
                Guid entityId = new Guid(id);

                if (string.IsNullOrEmpty(request.Folder))
                    request.Folder = request.CorrelationEntity;

                var existingbinary = repository.Find(null, x => x.Folder?.ToLower(null) == request.Folder?.ToLower(null) && x.Name.ToLower(null) == request.File?.FileName?.ToLower(null) && x.Id != entityId)?.Items?.FirstOrDefault();
                if (existingbinary != null)
                {
                    ModelState.AddModelError("BinaryObject", "Same file name already exists in the given folder");
                    return BadRequest(ModelState);
                }

                var existingBinaryObject = repository.GetOne(entityId);
                if (existingBinaryObject == null) return NotFound();

                string organizationId = existingBinaryObject.OrganizationId.ToString();
                if (!string.IsNullOrEmpty(organizationId))
                    organizationId = _binaryObjectManager.GetOrganizationId().ToString();

                string apiComponent = existingBinaryObject.CorrelationEntity;

                if (request.File == null)
                {
                    ModelState.AddModelError("Save", "No file uploaded");
                    return BadRequest(ModelState);
                }

                long size = request.File.Length;
                if (size <= 0)
                {
                    ModelState.AddModelError("Automation Upload", $"File size of file {request.File.FileName} cannot be 0");
                    return BadRequest(ModelState);
                }

                if (existingBinaryObject.Id != Guid.Empty && size > 0)
                {
                    existingBinaryObject.Name = request.Name;
                    existingBinaryObject.ContentType = request.File.ContentType;
                    existingBinaryObject.SizeInBytes = request.File.Length;
                    existingBinaryObject.Folder = request.Folder;

                    //update file in OpenBots.Server.Web using relative directory
                    _binaryObjectManager.Update(request.File, organizationId, apiComponent, Guid.Parse(id));
                    await _binaryObjectManager.UpdateEntity(request.File, existingBinaryObject.StoragePath, existingBinaryObject.Id.ToString(), apiComponent, existingBinaryObject.Folder, existingBinaryObject.Name);
                }
                else
                {
                    existingBinaryObject.Name = request.Name;
                    existingBinaryObject.Folder = request.Folder;
                    await base.PutEntity(id, existingBinaryObject);
                }

                await _webhookPublisher.PublishAsync("Files.FileUpdated", existingBinaryObject.Id.ToString(), existingBinaryObject.Name).ConfigureAwait(false);
                return Ok(existingBinaryObject);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Deletes a binary object with a specified id from the database
        /// </summary>
        /// <param name="id">Binary object id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when binary object is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if binary object id is null or empty Guid</response>
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
                Guid entityId = Guid.Parse(id);
                var existingBinaryObject = repository.GetOne(entityId);
                if (existingBinaryObject == null) return NotFound();
                await _webhookPublisher.PublishAsync("Files.FileDeleted", existingBinaryObject.Id.ToString(), existingBinaryObject.Name).ConfigureAwait(false);

                if (string.IsNullOrEmpty(existingBinaryObject.CorrelationEntity) || existingBinaryObject.CorrelationEntity == "BinaryObjectAPI")
                {
                    return Ok(base.DeleteEntity(id));
                }
                else
                {
                    Exception ex = new Exception();
                    ModelState.AddModelError("BinaryObject", ex.Message);
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }

        }

        /// <summary>
        /// Updates partial details of a binary object
        /// </summary>
        /// <param name="id">Binary object identifier</param>
        /// <param name="request">Value of the binary object to be updated</param>
        /// <response code="200">Ok, if update of binary object is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial binary object values have been updated</returns>
        [HttpPatch("{id}")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<BinaryObject> request)
        {
            try
            {
                Guid entityId = Guid.Parse(id);
                var existingBinaryObject = repository.GetOne(entityId);
                if (existingBinaryObject == null) return NotFound();

                await _webhookPublisher.PublishAsync("Files.FileUpdated", existingBinaryObject.Id.ToString(), existingBinaryObject.Name).ConfigureAwait(false);
                return await base.PatchEntity(id, request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}