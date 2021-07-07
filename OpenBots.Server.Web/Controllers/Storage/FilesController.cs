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
using OpenBots.Server.ViewModel.File;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for files
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/Storage/Drives")]
    [ApiController]
    [Authorize]
    [FeatureGate(MyFeatureFlags.Files)]
    public class FilesController : EntityController<StorageFile>
    {
        private readonly IFileManager _manager;

        /// <summary>
        /// FilesController constructor
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="storageFileRepository"></param>
        public FilesController(
            IFileManager manager,
            IStorageFileRepository storageFileRepository,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IMembershipManager membershipManager,
            IConfiguration configuration) : base(storageFileRepository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _manager = manager;
        }

        /// <summary>
        /// Provides a list of all files/folders
        /// </summary>
        /// <param name="type">Determines whether to retrieve all files (Files), folders (Folders), or both (null/empty)</param>
        /// <param name="driveId"></param>
        /// <param name="filter"></param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <response code="200">Ok, a paginated list of all files/folders</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response> 
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all files/folders</returns>
        [HttpGet("{driveId}")]
        [HttpGet("{driveId}/{type}")]
        [ProducesResponseType(typeof(PaginatedList<FileFolderViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult Get(string driveId, string type = null, string path = null,
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            try
            {
                ODataHelper<FileFolderViewModel> oDataHelper = new ODataHelper<FileFolderViewModel>();

                var oData = oDataHelper.GetOData(HttpContext, oDataHelper);
                bool? isFile = true;
                if (string.IsNullOrEmpty(type))
                    isFile = null;
                else if (type == "Files")
                    isFile = true;
                else if (type == "Folders")
                    isFile = false;
                var response = _manager.GetFilesFolders(driveId, isFile, oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take, path);

                return Ok(response); //return all files / folders / files and folders
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Gets count of storage files or folders in storage drive
        /// </summary>
        /// <param name="driveId"></param>
        /// <param name="filter"></param>
        /// <param name="type"></param>
        /// <response code="200">Ok, count of storage files/folders</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all storage files/folders</returns>
        [HttpGet("{driveId}/{type}/Count")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetCount(string driveId, string type,
        [FromQuery(Name = "$filter")] string filter = "")
        {
            try
            {
                int? count = null;
                if (type == "Files")
                    count = _manager.GetFileCount(driveId);
                else if (type == "Folders")
                    count = _manager.GetFolderCount(driveId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides file/folder details for a particular file/folder
        /// </summary>
        /// <param name="driveId"></param>
        /// <param name="id">File or folder id</param>
        /// <param name="type"></param>
        /// <response code="200">Ok, if a file/folder exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if file/folder id is not in proper format</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no file/folder exists for the given file/folder id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>File/folder details</returns>
        [HttpGet("{driveId}/{type}/{id}", Name = "GetFileFolder")]
        [ProducesResponseType(typeof(FileFolderViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetFileFolder(string id, string driveId, string type)
        {
            try
            {
                var fileFolder = _manager.GetFileFolder(id, driveId, type);

                var list = new PaginatedList<FileFolderViewModel>();
                list.Add(fileFolder);
                list.PageSize = 0;
                list.PageNumber = 0;
                list.TotalCount = 1;

                return Ok(list);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides file or folder details for a particular file or folder by storage path
        /// </summary>
        /// <param name="driveName"></param>
        /// <param name="path">File/folder path</param>
        /// <response code="200">Ok, if a file/folder exists with the given path</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if file/folder path is not in proper format</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no file/folder exists for the given file/folder path</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>File/folder details</returns>
        [HttpGet("Details/{driveName}", Name = "GetFileFolderByStoragePath")]
        [ProducesResponseType(typeof(FileFolderViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetFileFolderByStoragePath(string driveName, string path)
        {
            try
            {
                var fileFolder = _manager.GetFileFolderByStoragePath(path, driveName);

                var list = new PaginatedList<FileFolderViewModel>();
                list.Add(fileFolder);
                list.PageSize = 0;
                list.PageNumber = 0;
                list.TotalCount = 1;

                return Ok(list);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Uploads new file/folder in storage drive
        /// </summary>
        /// <param name="request"></param>
        /// <param name="driveId"></param>
        /// <param name="type"></param>
        /// <response code="200">Ok, new file/folder entity created and returned</response>
        /// <response code="400">Bad request, when the file/folder values are not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessable Entity, when a duplicate record is being entered</response>
        /// <returns>Newly created file/folder details</returns>
        [HttpPost("{driveId}/{type}")]
        [ProducesResponseType(typeof(FileFolderViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromForm] FileFolderViewModel request, string driveId, string type)
        {
            try
            {
                if (request.StoragePath.Contains("Assets") || request.StoragePath.Contains("Automations") ||
                    request.StoragePath.Contains("Email Attachments") || request.StoragePath.Contains("Queue Item Attachments"))
                    throw new EntityOperationException("Component files and folders cannot be added or changed in File Manager");

                if (request.IsFile == null && type == "Files")
                    request.IsFile = true;
                else if (request.IsFile == null && type == "Folders")
                    request.IsFile = false;
                var response = _manager.AddFileFolder(request, driveId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Export/Download a file
        /// </summary>
        /// <param name="driveId"></param>
        /// <param name="id">File id</param>
        /// <response code="200">Ok, if a file exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if file id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no file exists for the given id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Binary object file converted back to original file format</returns>
        [HttpGet("{driveId}/Files/{id}/Download")]
        [ProducesResponseType(typeof(MemoryStream), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Download(string id, string driveId)
        {
            try
            {
                var response = _manager.ExportFileFolder(id, driveId);
                return File(response?.Result?.Content, response?.Result?.ContentType, response?.Result?.Name);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Deletes a file or empty folder with a specified id from the database
        /// </summary>
        /// <param name="id">File or empty folder id to be deleted - throws bad request if null or empty Guid</param>
        /// <param name="driveId"></param>
        /// <param name="type"></param>
        /// <response code="200">Ok, when file or empty folder is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if binary object id is null or empty Guid</response>
        /// <response code="403">Forbidden</response>
        /// <returns>Ok response</returns>
        [HttpDelete("{driveId}/{type}/{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete(string id, string driveId, string type)
        {
            try
            {
                var file = _manager.GetFileFolder(id, driveId, type);
                if (file.StoragePath.Contains("Assets") || file.StoragePath.Contains("Automations") ||
                    file.StoragePath.Contains("Email Attachments") || file.StoragePath.Contains("Queue Item Attachments"))
                    throw new EntityOperationException("Component files and folders cannot be added or changed in File Manager");

                var fileFolder = _manager.DeleteFileFolder(id, driveId, type);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Renames a folder or file 
        /// </summary>
        /// <remarks>
        /// Provides an action to rename a folder or file, when the id and the new name are given
        /// </remarks>
        /// <param name="id">Folder or file id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Name to be updated</param>
        /// <param name="driveId"></param>
        /// <param name="type"></param>
        /// <response code="200">Ok, if the name for the given id has been updated</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{driveId}/{type}/{id}/Rename")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Rename(string id, [FromBody] FileFolderViewModel request, string driveId, string type)
        {
            try
            {
                var file = _manager.GetFileFolder(id, driveId, type);
                if (file.StoragePath.Contains("Assets") || file.StoragePath.Contains("Automations") ||
                    file.StoragePath.Contains("Email Attachments") || file.StoragePath.Contains("Queue Item Attachments"))
                    throw new EntityOperationException("Component files and folders cannot be added or changed in File Manager");

                if (string.IsNullOrEmpty(request.Name))
                {
                    ModelState.AddModelError("Rename", "No name given in request");
                    return BadRequest(ModelState);
                }
                string name = request.Name;
                var response = _manager.RenameFileFolder(id, name, driveId, type);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Moves a folder or file 
        /// </summary>
        /// <remarks>
        /// Provides an action to move a folder or file, when the id and the parent folder id are given
        /// </remarks>
        /// <param name="id">Folder or file id, produces bad request if id is null or ids don't match</param>
        /// <param name="parentId">Parent folder id to be moved to</param>
        /// <param name="driveId"></param>
        /// <param name="type"></param>
        /// <response code="200">Ok, if the file or folder for the given id has been updated</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{driveId}/{type}/{id}/Move/{parentId}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Move(string id, string parentId, string driveId, string type)
        {
            try
            {
                var file = _manager.GetFileFolder(id, driveId, type);
                if (file.StoragePath.Contains("Assets") || file.StoragePath.Contains("Automations") ||
                    file.StoragePath.Contains("Email Attachments") || file.StoragePath.Contains("Queue Item Attachments"))
                    throw new EntityOperationException("Component files and folders cannot be added or changed in File Manager");

                var response = _manager.MoveFileFolder(id, parentId, driveId, type);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Copies a folder or file 
        /// </summary>
        /// <remarks>
        /// Provides an action to copy a folder or file, when the id and the parent folder id are given
        /// </remarks>
        /// <param name="id">Folder or file id, produces bad request if id is null or ids don't match</param>
        /// <param name="parentId">Parent folder id to be copied to</param>
        /// <param name="driveId"></param>
        /// <param name="type"></param>
        /// <response code="200">Ok, if the file or folder for the given id has been copied</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the copied file or folder value</returns>
        [HttpPost("{driveId}/{type}/{id}/Copy/{parentId}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Copy(string id, string parentId, string driveId, string type)
        {
            try
            {
                var file = _manager.GetFileFolder(id, driveId, type);
                if (file.StoragePath.Contains("Assets") || file.StoragePath.Contains("Automations") ||
                    file.StoragePath.Contains("Email Attachments") || file.StoragePath.Contains("Queue Item Attachments"))
                    throw new EntityOperationException("Component files and folders cannot be added or changed in File Manager");

                var response = _manager.CopyFileFolder(id, parentId, driveId, type);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Replaces a file
        /// </summary>
        /// <remarks>
        /// Provides an action to replace a file, when file id and new details of the file are given
        /// </remarks>
        /// <param name="id">File id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">File details to be updated</param>
        /// <param name="driveId"></param>
        /// <param name="type"></param>
        /// <response code="200">Ok, if the file details for the given file id have been updated</response>
        /// <response code="400">Bad request, if the file id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value</returns>
        [HttpPut("{driveId}/{type}/{id}/Replace")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Replace(string id, string type, string driveId, [FromForm] FileFolderViewModel request)
        {
            try
            {
                if (type == "Folders")
                    throw new EntityOperationException("Folders cannot be replaced");

                request.Id = Guid.Parse(id);
                if (request.StorageDriveId == null || request.StorageDriveId == Guid.Empty)
                    request.StorageDriveId = Guid.Parse(driveId);
                var existingFile = _manager.UpdateFile(request);
                return Ok(existingFile);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}