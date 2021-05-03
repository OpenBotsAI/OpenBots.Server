using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement.Mvc;
using OpenBots.Server.Business;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Exceptions;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.Model.Options;
using OpenBots.Server.Security;
using OpenBots.Server.Web.Webhooks;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for storage drives
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/Storage/[controller]")]
    [ApiController]
    [Authorize]
    [FeatureGate(MyFeatureFlags.Files)]
    public class DrivesController : EntityController<StorageDrive>
    {
        private readonly IStorageDriveRepository _repository;
        private readonly IFileManager _manager;
        private readonly IWebhookPublisher _webhookPublisher;
        private readonly IOrganizationManager _organizationManager;

        /// <summary>
        /// DrivesController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="membershipManager"></param>
        /// <param name="configuration"></param>
        /// <param name="fileManager"></param>
        /// <param name="webhookPublisher"></param>
        /// <param name="organizationManager"></param>
        public DrivesController(
            IStorageDriveRepository repository,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IMembershipManager membershipManager,
            IConfiguration configuration,
            IFileManager fileManager,
            IWebhookPublisher webhookPublisher,
            IOrganizationManager organizationManager) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _repository = repository;
            _manager = fileManager;
            _webhookPublisher = webhookPublisher;
            _organizationManager = organizationManager;
        }

        /// <summary>
        /// Provides a list of all storage drives
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of all storage drives</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response> 
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all storage drives</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<StorageDrive>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            try
            {
                var drives = base.GetMany();

                return Ok(drives); //return all storage drives
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Gets count of storage drives in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, count of storage drives</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all storage drives</returns>
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
                int? count = base.Count();
                return Ok(count);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides storage drive details by drive id
        /// </summary>
        /// <param name="id">Storage drive id</param>
        /// <response code="200">Ok, if a storage drive exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if storage drive id is not in proper format</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no storage drive exists for the given drive id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Storage drive details</returns>
        [HttpGet("DriveDetails/{id}", Name = "GetDrive")]
        [ProducesResponseType(typeof(StorageDrive), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetDrive(string id)
        {
            try
            {
                return base.GetEntity(id).Result;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides storage drive details by drive name
        /// </summary>
        /// <param name="driveName">Storage drive name</param>
        /// <response code="200">Ok, if a storage drive exists with the given name</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if storage drive name is not in proper format</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no storage drive exists for the given name</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Storage drive details</returns>
        [HttpGet("DriveDetailsByName/{driveName}", Name = "GetDriveByName")]
        [ProducesResponseType(typeof(StorageDrive), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetDriveByName(string driveName)
        {
            try
            {
                return Ok(_manager.GetDriveByName(driveName));
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Creates new storage drive
        /// </summary>
        /// <param name="drive"></param>
        /// <response code="200">Ok, new storage drive entity created and returned</response>
        /// <response code="400">Bad request, when the storage drive values are not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity, when a duplicate record is being entered</response>
        /// <returns>Newly created storage drive details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(StorageDrive), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] StorageDrive drive)
        {
            try
            {
                var response = _manager.AddStorageDrive(drive);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates a storage drive
        /// </summary>
        /// <remarks>
        /// Provides an action to update a storage drive, when the id is given
        /// </remarks>
        /// <param name="id">Storage drive id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Storage drive to be updated</param>
        /// <response code="200">Ok, if the storage drive for the given id has been updated</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(StorageDrive), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] StorageDrive request)
        {
            try
            {
                if (request.Id == null || request.Id == Guid.Empty)
                    request.Id = Guid.Parse(id);

                if (request.OrganizationId == null || request.OrganizationId == Guid.Empty)
                    request.OrganizationId = _organizationManager.GetDefaultOrganization().Id;
                _manager.CheckDefaultDrive(request, request.OrganizationId);

                if (string.IsNullOrEmpty(request.StoragePath))
                    request.StoragePath = request.Name;

                await _webhookPublisher.PublishAsync("Files.DriveUpdated", id, request.Name).ConfigureAwait(false);
                return await base.PutEntity(id, request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Deletes a storage drive with a specified id from the database
        /// </summary>
        /// <param name="id">Storage drive id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when empty storage drive is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if storage drive id is null or empty Guid</response>
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
                var drive = _repository.Find(null, q => q.Id.ToString() == id).Items.FirstOrDefault();
                if (drive.StorageSizeInBytes > 0)
                    throw new EntityOperationException("Storage drive cannot be deleted because it contains files");
                if (drive.IsDefault == true)
                    throw new EntityOperationException("Storage drive cannot be deleted because it is set as default drive");

                base.DeleteEntity(id);
                await _webhookPublisher.PublishAsync("Files.DriveDeleted", drive.Id.ToString(), drive.Name).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides storage drive names
        /// </summary>
        /// <response code="200">Ok, if the storage drive names exist</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if storage drive name is not in proper format</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no storage drive names exist</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Storage drive names</returns>
        [HttpGet("driveNames", Name = "GetDriveNames")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetDriveNames(string adapterType)
        {
            try
            {
                var driveNames = _manager.GetDriveNames(adapterType);
                return Ok(driveNames);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}