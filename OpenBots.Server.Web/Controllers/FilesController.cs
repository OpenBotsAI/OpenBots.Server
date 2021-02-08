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
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    [FeatureGate(MyFeatureFlags.Files)]
    public class FilesController : EntityController<ServerFile>
    {
        private readonly IFileManager _manager;

        //TODO: add folder / file (google/amazon/azure)
        //TODO: upload / download a file (google/amazon/azure)
        //TODO: delete a folder / file (google/amazon/azure)

        /// <summary>
        /// FilesController constructor
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        public FilesController (
            IFileManager manager,
            IServerFileRepository serverFileRepository,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IMembershipManager membershipManager,
            IConfiguration configuration) : base(serverFileRepository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            _manager = manager;
        }

        /// <summary>
        /// Provides a list of all files/folders
        /// </summary>
        /// <param name="file">Determines whether to retrieve all files (true), folders (false), or both (null/empty)</param>
        /// <param name="driveName"></param>
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
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<FileFolderViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult Get(string driveName = null, string file = null,
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0)
        {
            try
            {
                ODataHelper<FileFolderViewModel> oDataHelper = new ODataHelper<FileFolderViewModel>();

                var oData = oDataHelper.GetOData(HttpContext, oDataHelper);
                bool? isFile = Convert.ToBoolean(file);
                if (file == null)
                    isFile = null;
                var filesFolders = _manager.GetFilesFolders(isFile, driveName, oData.Predicate, oData.PropertyName, oData.Direction, oData.Skip, oData.Take);

                return Ok(filesFolders); //return all files/folders
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Gets count of server files in database
        /// </summary>
        /// <param name="driveName"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, count of server files</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all server files</returns>
        [HttpGet("count")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetFileCount(string driveName = null,
        [FromQuery(Name = "$filter")] string filter = "")
        {
            try
            {
                int? count = _manager.GetFileCount(driveName);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Gets count of server folders in database
        /// </summary>
        /// <param name="driveName"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, count of server folders</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all server folders</returns>
        [HttpGet("count/folder")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> GetFolderCount(string driveName = null,
        [FromQuery(Name = "$filter")] string filter = "")
        {
            try
            {
                int? count = _manager.GetFolderCount(driveName);
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
        /// <param name="driveName"></param>
        /// <param name="id">File or folder id</param>
        /// <response code="200">Ok, if a file/folder exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if file/folder id is not in proper format</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no file/folder exists for the given file id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>File details</returns>
        [HttpGet("{id}", Name = "GetFileFolder")]
        [ProducesResponseType(typeof(FileFolderViewModel), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetFileFolder(string id, string driveName = null)
        {
            try
            {
                var fileFolder = _manager.GetFileFolder(id, driveName);

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
        /// Provides server drive details for local storage
        /// </summary>
        /// <param name="driveName"></param>
        /// <response code="200">Ok, if the server drive exists</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if server drive entity is not in proper format</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no server drive exists</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Server drive details</returns>
        [HttpGet("drive", Name = "GetDrive")]
        [ProducesResponseType(typeof(ServerDrive), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetDrive(string driveName = null)
        {
            try
            {
                var drive = _manager.GetDrive(driveName);
                return Ok(drive);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Uploads new file/folder in server drive
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, new file/folder entity created and returned</response>
        /// <response code="400">Bad request, when the file/folder values are not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created file/folder details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ServerFolder), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromForm] FileFolderViewModel request, string driveName = null)
        {
            try
            {
                if (request.IsFile == null)
                    request.IsFile = true;
                var response = _manager.AddFileFolder(request, driveName);
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
        /// <param name="driveName"></param>
        /// <param name="id">File id</param>
        /// <response code="200">Ok, if a file exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if file id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no file exists for the given id</response>
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
        public async Task<IActionResult> Download(string id, string driveName = null)
        {
            try
            {
                var response = _manager.ExportFileFolder(id, driveName);
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
        /// <param name="driveName"></param>
        /// <response code="200">Ok, when file or empty folder is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if binary object id is null or empty Guid</response>
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
                _manager.DeleteFileFolder(id, driveName);
                return Ok();
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        //TODO: update size of folder and all parent folders when file/folder is updated
        //TODO additional api calls:
        //update file/folder details in server drive (rename, move, copy)
        //add additional server drive?
        //update server drive details?
        //delete server drive?
        //get file attributes for a file
    }
}