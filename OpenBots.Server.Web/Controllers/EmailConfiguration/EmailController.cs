using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenBots.Server.Business;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Configuration;
using OpenBots.Server.Model.Core;

namespace OpenBots.Server.Web.Controllers.EmailConfiguration
{
    /// <summary>
    /// Controller to send an email
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailController : ControllerBase
    {
        private readonly IEmailManager manager;

        /// <summary>
        /// EmailController constructor
        /// </summary>
        /// <param name="manager"></param>
        public EmailController(IEmailManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// Sends a new email
        /// </summary>
        /// <remarks>
        /// Creates an EmailMessage to send to an email address
        /// </remarks>
        /// <param name="emailMessage"></param>
        /// <response code="200">Ok, new email message created and sent</response>
        /// <response code="400">Bad request, when the email message value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Ok response</returns>
        [HttpPost]
        [ProducesResponseType(typeof(EmailAccount), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        [HttpPost("{accountName}")]
        public void Post(string accountName, [FromBody] EmailMessage emailMessage)
        {
            manager.SendEmailAsync(emailMessage, accountName);
        }
    }
}
