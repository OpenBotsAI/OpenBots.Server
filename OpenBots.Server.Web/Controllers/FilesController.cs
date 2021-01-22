using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement.Mvc;
using OpenBots.Server.Business;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.File;
using OpenBots.Server.Model.Options;
using OpenBots.Server.Security;
using OpenBots.Server.WebAPI.Controllers;
using Syncfusion.EJ2.FileManager.Base;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for files
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    //[Authorize]
    [AllowAnonymous]
    [FeatureGate(MyFeatureFlags.Files)]
    public class FilesController : EntityController<ServerFile>
    {
        private readonly IFileManager manager;

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
            this.manager = manager;
        }

        /// <summary>
        /// Conduct basic file and/or folder operations: read, delete, copy, move, details, create, search, and rename
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Object as a result of the operation type</returns>
        [HttpOptions("FileOperations")]
        [HttpPost("FileOperations")]
        [HttpPut("FileOperations")]
        [HttpDelete("FileOperations")]
        [HttpGet("FileOperations")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult FileOperations([FromBody] FileManagerDirectoryContent args)
        {
            try
            {
                return Ok(manager.LocalFileStorageOperation(args));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("File Operations", ex.Message);
                return BadRequest(ModelState);
            }
        }
                    
        /// <summary>
        /// Uploads the file(s) into a specified path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="uploadFiles"></param>
        /// <param name="action"></param>
        /// <returns>200 OK response</returns>
        [HttpOptions("Upload")]
        [HttpPost("Upload")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult Upload(string path, IList<IFormFile> uploadFiles, string action)
        {
            try
            {
                FileManagerResponse uploadResponse = manager.UploadFile(path, uploadFiles, action);

            if (uploadResponse.Error != null)
            {
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
            }

            return Content("");
        }
            catch (Exception ex)
            {
                ModelState.AddModelError("Upload File", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Downloads the selected file(s) and folder(s)
        /// </summary>
        /// <param name="downloadInput"></param>
        /// <returns>Selected file(s) and folder(s) to be downloaded</returns>
        [HttpOptions("Download")]
        [HttpPost("Download")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult Download(string downloadInput)
        {
            try
            {
                return Ok(manager.DownloadFile(downloadInput));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Download File", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Gets the image(s) from the given path
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Requested image from the given path</returns>
        [HttpOptions("GetImage")]
        [HttpGet("GetImage")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult GetImage(FileManagerDirectoryContent args)
        {
            try
            {
                return Ok(manager.GetImage(args));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Get Image", ex.Message);
                return BadRequest(ModelState);
            }
        }
    }
}