using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement.Mvc;
using Newtonsoft.Json;
using OpenBots.Server.Business;
using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.File;
using OpenBots.Server.Model.Options;
using OpenBots.Server.Security;
using OpenBots.Server.WebAPI.Controllers;
using Syncfusion.EJ2.FileManager.Base;
using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
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
    [Authorize]
    [FeatureGate(MyFeatureFlags.Files)]
    public class FilesController : EntityController<ServerFile>
    {
        private readonly IFileManager manager;
        public PhysicalFileProvider operation;
        public string basePath;
        public string root = "Files";

        //TODO: Add webhooks to controllers
        //TODO: add folder, add file (google/amazon/azure)
        //TODO: upload / download a file (google/amazon/azure)
        //TODO: delete an entire folder or an individual file (google/amazon/azure)
        //TODO: edit storage amount based on operation type

        /// <summary>
        /// FilesController constructor
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="serverFileRepository"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="hostingEnvironment"></param>
        public FilesController (
            IFileManager manager,
            IServerFileRepository serverFileRepository,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IMembershipManager membershipManager,
            IConfiguration configuration,
            IHostingEnvironment hostingEnvironment) : base(serverFileRepository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.manager = manager;
            this.basePath = hostingEnvironment.ContentRootPath;
            this.operation = new PhysicalFileProvider();
            this.operation.RootFolder(this.basePath + "\\" + this.root);
        }

        /// <summary>
        /// Conduct basic file and/or folder operations: read, delete, copy, move, details, create, search, and rename
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Object as a result of the operation</returns>
        [HttpOptions("FileOperations")]
        [HttpPost("FileOperations")]
        [HttpPut("FileOperations")]
        [HttpDelete("FileOperations")]
        [HttpGet("FileOperations")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public object FileOperations([FromBody] FileManagerDirectoryContent args)
        {
            if (args.Action == "delete" || args.Action == "rename")
            {
                if ((args.TargetPath == null) && (args.Path == ""))
                {
                    FileManagerResponse response = new FileManagerResponse();
                    response.Error = new ErrorDetails { Code = "401", Message = "Restricted to modify the root folder." };
                    return this.operation.ToCamelCase(response);
                }
            }
            switch (args.Action)
            {
                case "read":
                    // reads the file(s) or folder(s) from the given path.
                    return this.operation.ToCamelCase(this.operation.GetFiles(args.Path, args.ShowHiddenItems));
                case "delete":
                    // deletes the selected file(s) or folder(s) from the given path.
                    if (args.IsFile.Equals(true))
                    {
                        //TODO: delete ServerFile entity
                        repository.GetOne(Guid.Parse(args.Id));
                    }
                    else
                    {
                        //TODO: delete ServerFolder entity
                    }
                    //TODO: subtract SizeInBytes property in ServerDrive
                    return this.operation.ToCamelCase(this.operation.Delete(args.Path, args.Names));
                case "copy":
                    // copies the selected file(s) or folder(s) from a path and then pastes them into a given target path.
                    if (args.IsFile.Equals(true))
                    {
                        //TODO: update ServerFile entity
                    }
                    else
                    {
                        //TODO: update ServerFolder entity
                    }
                    //TODO: add to SizeInBytes property in ServerDrive
                    return this.operation.ToCamelCase(this.operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
                case "move":
                    // cuts the selected file(s) or folder(s) from a path and then pastes them into a given target path.
                    if (args.IsFile.Equals(true))
                    {
                        //TODO: update ServerFile entity
                    }
                    else
                    {
                        //TODO: update ServerFolder entity
                    }
                    return this.operation.ToCamelCase(this.operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
                case "details":
                    // gets the details of the selected file(s) or folder(s).
                    return this.operation.ToCamelCase(this.operation.Details(args.Path, args.Names, args.Data));
                case "create":
                    // creates a new folder in a given path.
                    
                    //TODO: add ServerFolder entity
                    //TODO: add to SizeInBytes property in ServerDrive

                    return this.operation.ToCamelCase(this.operation.Create(args.Path, args.Name));
                case "search":
                    // gets the list of file(s) or folder(s) from a given path based on the searched key string.
                    return this.operation.ToCamelCase(this.operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive));
                case "rename":
                    // renames a file or folder.
                    if (args.IsFile.Equals(true))
                    {
                        //TODO: update ServerFile entity
                    }
                    else
                    {
                        //TODO: update ServerFoler entity
                    }
                    return this.operation.ToCamelCase(this.operation.Rename(args.Path, args.Name, args.NewName));
            }
            return null;
        }

        /// <summary>
        /// Uploads the file(s) into a specified path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="uploadFiles"></param>
        /// <param name="action"></param>
        /// <returns>Content</returns>
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
            //TODO: add try/catch
            //TODO: create ServerFile entity
            //TODO: add to SizeInBytes property in ServerDrive
            FileManagerResponse uploadResponse = operation.Upload(path, uploadFiles, action, null);
            if (uploadResponse.Error != null)
            {
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
            }
            return Content("");
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
            //TODO: add try/catch
            //TODO: update ServerFile FileAttributes
            FileManagerDirectoryContent args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(downloadInput);
            return operation.Download(args.Path, args.Names, args.Data);
        }

        /// <summary>
        /// Gets the image(s) from the given path
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
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
            //TODO: try/catch
            //TODO: update ServerFile FileAttributes
            return this.operation.GetImage(args.Path, args.Id, false, null, null);
        }
    }
}