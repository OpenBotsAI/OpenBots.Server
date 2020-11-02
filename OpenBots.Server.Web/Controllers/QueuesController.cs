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
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for Queues
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class QueuesController : EntityController<Queue>
    {
        /// <summary>
        /// QueuesController constructor
        /// </summary>
        IQueueManager queueManager;
        public QueuesController(
            IQueueRepository repository,
            IQueueManager queueManager,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration) 
        {
            this.queueManager = queueManager;
        }

        /// <summary>
        /// Provides a list of all queues
        /// </summary>
        /// <response code="200">Ok, a paginated list of all queues</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all queues</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<Queue>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<Queue> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Gets count of queues in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok with count of all queues</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all queues</returns>
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
        /// Provides queue details for a particular queue id
        /// </summary>
        /// <param name="id">Queue id</param>
        /// <response code="200">Ok, if queue exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if queue id is not in proper format or proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no queue exists for the given id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Queue details for the given id</returns>
        [HttpGet("{id}", Name = "GetQueue")]
        [ProducesResponseType(typeof(Queue), StatusCodes.Status200OK)]
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
        /// Adds a new queue
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, new queue created and returned</response>
        /// <response code="400">Bad request, when the queue value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created unique queue id with route name</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Queue), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] Queue request)
        {
            if (request == null)
            {
                ModelState.AddModelError("Save", "No data passed");
                return BadRequest(ModelState);
            }

            var queue = repository.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null))?.Items?.FirstOrDefault();
            if (queue != null)
            {
                ModelState.AddModelError("Queue", "Queue Name Already Exists");
                return BadRequest(ModelState);
            }
            
            try
            {
                return await base.PostEntity(request);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Updates a queue 
        /// </summary>
        /// <remarks>
        /// Provides an action to update an queue, when queue id and the new details of the queue are given
        /// </remarks>
        /// <param name="id">Queue id, produces bad request if id is null or ids don't match</param>
        /// <param name="request">Queue details to be updated</param>
        /// <response code="200">Ok, if the queue details for the given queue id have been updated</response>
        /// <response code="400">Bad request, if the queue id is null or ids don't match</response>
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
        public async Task<IActionResult> Put(string id, [FromBody] QueueViewModel request)
        {
            try
            {
                Guid entityId = new Guid(id);
                
                var existingQueue = repository.GetOne(entityId);
                if (existingQueue == null) return NotFound();

                var queue = repository.Find(null, d => d.Name.ToLower(null) == request.Name.ToLower(null) && d.Id != entityId)?.Items?.FirstOrDefault();
                if (queue != null && existingQueue.Id != entityId)
                {
                    ModelState.AddModelError("Queue", "Queue Name Already Exists");
                    return BadRequest(ModelState);
                }
                               
                existingQueue.Description = request.Description;
                existingQueue.Name = request.Name;
                existingQueue.MaxRetryCount = request.MaxRetryCount;

                return await base.PutEntity(id, existingQueue);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Queue", ex.Message);
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Deletes a queue with a specified id from the queues
        /// </summary>
        /// <param name="id">Queue id to be deleted - throws bad request if null or empty Guid</param>
        /// <response code="200">Ok, when queue is soft deleted, (isDeleted flag is set to true in database)</response>
        /// <response code="400">Bad request, if queue id is null or empty Guid</response>
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
            bool lockedChildExists = queueManager.CheckReferentialIntegrity(id);
            if (lockedChildExists)
            {
                ModelState.AddModelError("Delete Agent", "Referential Integrity in QueueItems table, please remove any locked items associated with this queue first");
                return BadRequest(ModelState);
            }
            return await base.DeleteEntity(id);
        }

        /// <summary>
        /// Updates partial details of queue
        /// </summary>
        /// <param name="id">Queue identifier</param>
        /// <param name="request">Value of the queue to be updated</param>
        /// <response code="200">Ok, if update of queue is successful</response>
        /// <response code="400">Bad request, if the id is null or ids don't match</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity, validation error</response>
        /// <returns>Ok response, if the partial queue values have been updated</returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [Produces("application/json")]
        public async Task<IActionResult> Patch(string id,
            [FromBody] JsonPatchDocument<Queue> request)
        {
            Guid entityId = new Guid(id);
            for (int i = 0; i < request.Operations.Count; i++)
            {
                if (request.Operations[i].op.ToString().ToLower() == "replace" && request.Operations[i].path.ToString().ToLower() == "/name")
                {
                    var queue = repository.Find(null, d => d.Name.ToLower(null) == request.Operations[i].value.ToString().ToLower(null) && d.Id != entityId)?.Items?.FirstOrDefault();
                    if (queue != null)
                    {
                        ModelState.AddModelError("Queue", "Queue Name Already Exists");
                        return BadRequest(ModelState);
                    }
                }
            }
            return await base.PatchEntity(id, request);
        }
    }
}
