using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Webhooks;
using OpenBots.Server.Security;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenBots.Server.Web.Controllers.WebHooksApi
{
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    public class WebhooksClientController
    {
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] WebhookPayload payload)
        {
            try
            {
                return new OkObjectResult("Good Job");
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }
    }
}
