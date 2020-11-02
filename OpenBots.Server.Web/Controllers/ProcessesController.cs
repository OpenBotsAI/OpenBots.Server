using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using OpenBots.Server.ViewModel;
using OpenBots.Server.WebAPI.Controllers;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for Studio processes
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class ProcessesController : EntityController<Process>
    {
        private readonly IProcessManager manager;
        private readonly IBinaryObjectManager binaryObjectManager;
        private readonly IBinaryObjectRepository binaryObjectRepo;

        /// <summary>
        /// Process Controller constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="manager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        public ProcessesController(
            IProcessRepository repository,
            IProcessManager manager,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IBinaryObjectRepository binaryObjectRepo,
            IBinaryObjectManager binaryObjectManager,
            IConfiguration configuration) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.manager = manager;
            this.binaryObjectRepo = binaryObjectRepo;
            this.binaryObjectManager = binaryObjectManager;
        }

        /// <summary>
        /// Provides a list of all processes
        /// </summary>
        /// <response code="200">Ok, a paginated list of all processes</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all processes</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<ProcessViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<ProcessViewModel> Get(
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0
        )
        {
            return base.GetMany<ProcessViewModel>();
        }

        /// <summary>
        /// Gets count of processes in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, a count of all processes</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all processes</returns>
        [HttpGet("count")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<int?> GetCount(
        [FromQuery(Name = "$filter")] string filter = "")
        {
            return base.Count();
        }

        /// <summary>
        /// Get process by id
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if a process exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if process id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no process exists for the given process id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Process entity</returns>
        [HttpGet("{id}", Name = "GetProcess")]
        [ProducesResponseType(typeof(PaginatedList<Process>), StatusCodes.Status200OK)]
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
        /// Create a new process entity
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, new process created and returned</response>
        /// <response code="400">Bad request, when the process value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created process details</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Process), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] Process request)
        {
            try
            {
                Guid versionId = Guid.Empty;
                if (request.VersionId == null || request.VersionId == Guid.Empty)
                    versionId = Guid.NewGuid();
                else
                    versionId = request.VersionId;
                    manager.AssignProcessProperties(request, versionId);

                var response = await base.PostEntity(request);

                return response;
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Create a new binary object and upload process file
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <response code="200">Ok, process updated and returned</response>
        /// <response code="400">Bad request, when the process value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly updated process detaills</returns>
        [HttpPost("{id}/upload")]
        [ProducesResponseType(typeof(Process), StatusCodes.Status200OK)]
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
                    ModelState.AddModelError("Process Upload", "No process uploaded");
                    return BadRequest(ModelState);
                }

                var process = repository.GetOne(Guid.Parse(id));
                string organizationId = binaryObjectManager.GetOrganizationId();
                string apiComponent = "ProcessAPI";

                BinaryObject binaryObject = new BinaryObject();
                binaryObject.Name = file.FileName;
                binaryObject.Folder = apiComponent;
                binaryObject.CreatedOn = DateTime.UtcNow;
                binaryObject.CreatedBy = applicationUser?.UserName;
                binaryObject.CorrelationEntityId = process.Id;
                binaryObjectRepo.Add(binaryObject);

                string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObject.Id.ToString());

                binaryObjectManager.Upload(file, organizationId, apiComponent, binaryObject.Id.ToString());
                binaryObjectManager.SaveEntity(file, filePath, binaryObject, apiComponent, organizationId);

                process.BinaryObjectId = (Guid)binaryObject.Id;
                repository.Update(process);

                return Ok(process);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Asset", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Update process with file 
        /// </summary>
        /// <remarks>
        /// Provides an action to update a process, when process id and the new details of process are given
        /// </remarks>
        /// <param name="id">Process id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Process details to be updated</param>
        /// <response code="200">Ok, if the process details for the given process id have been updated</response>
        /// <response code="400">Bad request, if the process id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value details</returns>
        [HttpPost("{id}/update")]
        [ProducesResponseType(typeof(Process), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Update(string id, [FromForm] ProcessViewModel request)
        {
            Guid entityId = new Guid(id);
            var existingProcess = repository.GetOne(entityId);
            if (existingProcess == null) return NotFound();

            if (request == null)
            {
                ModelState.AddModelError("Save", "No data passed");
                return BadRequest(ModelState);
            }

            long size = request.File.Length;
            string binaryObjectId = existingProcess.BinaryObjectId.ToString();
            var binaryObject = binaryObjectRepo.GetOne(Guid.Parse(binaryObjectId));
            string organizationId = binaryObject.OrganizationId.ToString();

            if (!string.IsNullOrEmpty(organizationId))
                organizationId = manager.GetOrganizationId().ToString();

            try
            {
                BinaryObject newBinaryObject = new BinaryObject();
                if (existingProcess.BinaryObjectId != Guid.Empty && size > 0)
                {
                    string apiComponent = "ProcessAPI";
                    //Update file in OpenBots.Server.Web using relative directory
                    newBinaryObject.Id = Guid.NewGuid();
                    newBinaryObject.Name = request.File.FileName;
                    newBinaryObject.Folder = apiComponent;
                    newBinaryObject.StoragePath = Path.Combine("BinaryObjects", organizationId, apiComponent, newBinaryObject.Id.ToString());
                    binaryObjectRepo.Add(newBinaryObject);
                    binaryObjectManager.Upload(request.File, organizationId, apiComponent, newBinaryObject.Id.ToString());
                    binaryObjectManager.SaveEntity(request.File, newBinaryObject.StoragePath, newBinaryObject, apiComponent, organizationId);
                }

                //Update process entity
                Process response = existingProcess;
                if (existingProcess.Name.Trim().ToLower() != request.Name.Trim().ToLower() || existingProcess.Status.Trim().ToLower() != request.Status?.Trim().ToLower()) 
                {
                    Guid versionId = existingProcess.VersionId;
                    existingProcess.BinaryObjectId = (Guid)newBinaryObject.Id;
                    response = manager.UpdateProcess(existingProcess, request, versionId);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Update a Process 
        /// </summary>
        /// <remarks>
        /// Provides an action to update a process, when process id and the new details of process are given
        /// </remarks>
        /// <param name="id">Process id, produces bad request if id is null or ids don't match</param>
        /// <param name="value">Process details to be updated</param>
        /// <response code="200">Ok, if the process details for the given process id have been updated</response>
        /// <response code="400">Bad request, if the process id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Ok response with the updated value details</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Put(string id, [FromBody] ProcessViewModel value)
        {
            try
            {
                Guid entityId = new Guid(id);

                var existingProcess = repository.GetOne(entityId);
                if (existingProcess == null) return NotFound();

                existingProcess.Name = value.Name;

                return await base.PutEntity(id, existingProcess);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Process", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Updates partial details of a process
        /// </summary>
        /// <param name="id">Process identifier</param>
        /// <param name="request">Value of the process to be updated</param>
        /// <response code="200">Ok, if update of process is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity ,validation error</response>
        /// <returns>Ok response, if the partial process values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<Process> request)
        {
            return await base.PatchEntity(id, request);
        }

        /// <summary>
        /// Export/download a process
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Ok, if a process exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if process id is not in proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no process exists for the given process id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Downloaded process file</returns>        
        [HttpGet("{id}/Export", Name = "ExportProcess")]
        [ProducesResponseType(typeof(MemoryStream), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Export(string id)
        {
            try
            {
                Guid processId;
                Guid.TryParse(id, out processId);

                Process process = repository.GetOne(processId);
               
                if (process == null || process.BinaryObjectId == null || process.BinaryObjectId == Guid.Empty)
                {
                    ModelState.AddModelError("Process Export", "No process or process file found");
                    return BadRequest(ModelState);
                }

                var fileObject = manager.Export(process.BinaryObjectId.ToString());
                return File(fileObject?.Result?.BlobStream, fileObject?.Result?.ContentType, fileObject?.Result?.Name);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Delete process with a specified id from list of processes
        /// </summary>
        /// <param name="id">Process id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when process is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if Process id is null or empty Guid</response>
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
                Guid processId;
                Guid.TryParse(id, out processId);

                bool response = manager.DeleteProcess(processId);

                if (response)
                    return Ok();
                else
                {
                    ModelState.AddModelError("Process Delete", "An error occured while deleting a process");
                    return BadRequest(ModelState);
                }
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Lookup list of all processes
        /// </summary>
        /// <response code="200">Ok, a lookup list of all processes</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Lookup list of all processes</returns>
        [HttpGet("GetLookup")]
        [ProducesResponseType(typeof(List<JobProcessLookup>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public List<JobProcessLookup> GetLookup()
        {
            var processList = repository.Find(null, x => x.IsDeleted == false);
            var processLookup = from p in processList.Items.GroupBy(p => p.Id).Select(p => p.First()).ToList()
                                select new JobProcessLookup
                                {
                                    ProcessId = (p == null || p.Id == null) ? Guid.Empty : p.Id.Value,
                                    ProcessName = p?.Name,
                                    ProcessNameWithVersion = string.Format("{0} (v{1})", p?.Name.Trim(), p?.Version) 
                                };

            return processLookup.ToList();
        }
    }
}
