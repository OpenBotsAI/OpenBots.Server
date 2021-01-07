using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Attributes;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.WebAPI.Controllers;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Web
{
    /// <summary>
    /// Controller for Automation Logs
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class AutomationLogsController : EntityController<AutomationLog>
    {
        IAutomationLogManager automationLogManager;

        /// <summary>
        /// AutomationLogsController constructor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="userManager"></param>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="automationLogManager"></param>
        public AutomationLogsController(
            IAutomationLogRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IAutomationLogManager automationLogManager,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.automationLogManager = automationLogManager;
            this.automationLogManager.SetContext(SecurityContext);
        }

        /// <summary>
        /// Provides a list of all automation logs
        /// </summary>
        /// <response code="200">Ok, a paginated list of all automation logs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>   
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all automation logs</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<AutomationLog>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<AutomationLog> Get(
            [FromQuery(Name = "$filter")] string filter = "",
            [FromQuery(Name = "$orderby")] string orderBy = "",
            [FromQuery(Name = "$top")] int top = 100,
            [FromQuery(Name = "$skip")] int skip = 0
            )
        {
            return base.GetMany();
        }

        /// <summary>
        /// Provides a count of automation logs
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, total count of automation logs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>        
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all automation logs</returns>
        [HttpGet("Count")]
        [ProducesResponseType(typeof(int?), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public int? Count(
            [FromQuery(Name = "$filter")] string filter = "")
        {
            return base.Count();
        }

        /// <summary>
        /// Provides a automation log's details for a particular automation log id
        /// </summary>
        /// <param name="id">automation log id</param>
        /// <response code="200">Ok, if a automation log exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if automation log id is not in the proper format or a proper Guid</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no automation log exists for the given automation log id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Automation log details for the given id</returns>
        [HttpGet("{id}", Name = "GetAutomationLog")]
        [ProducesResponseType(typeof(AutomationLog), StatusCodes.Status200OK)]
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
        /// Exports automation logs into a downloadable file
        /// </summary>
        /// <param name="fileType">Specifies the file type to be downloaded: csv, zip, or json</param>
        /// <response code="200">Ok, if a log exists with the given filters</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Downloadable file</returns>
        [HttpGet("export/{filetype?}")]
        [Produces("text/csv", "application/zip", "application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<object> Export(
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0, string fileType = "")
        {
            try
            {
                //Determine top value
                int maxExport = int.Parse(config["App:MaxExportRecords"]);
                top = top > maxExport | top == 0 ? maxExport : top; //If $top is greater than max or equal to 0 use maxExport value
                ODataHelper<AutomationLog> oData = new ODataHelper<AutomationLog>();
                string queryString = HttpContext.Request.QueryString.Value;

                oData.Parse(queryString);
                oData.Top = top;

                var automationLogsJson = base.GetMany(oData : oData);
                string csvString = automationLogManager.GetJobLogs(automationLogsJson.Items.ToArray());
                var csvFile = File(new System.Text.UTF8Encoding().GetBytes(csvString), "text/csv", "Logs.csv");

                switch (fileType.ToLower())
                {
                    case "csv":
                        return csvFile;

                    case "zip":
                        var zippedFile = automationLogManager.ZipCsv(csvFile);
                        const string contentType = "application/zip";
                        HttpContext.Response.ContentType = contentType;
                        var zipFile = new FileContentResult(zippedFile.ToArray(), contentType)
                        {
                            FileDownloadName = "AutomationLogs.zip"
                        };

                        return zipFile;

                    case "json":
                        return automationLogsJson;
                }
                return csvFile;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Export", ex.Message);
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Adds a new automation log to the existing automation logs
        /// </summary>
        /// <remarks>
        /// Adds the automation log with unique automation log id to the existing automation logs
        /// </remarks>
        /// <param name="request"></param>
        /// <response code="200">Ok, new automation log created and returned</response>
        /// <response code="400">Bad request, when the automation log value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        ///<response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Newly created unique automation log id with route name</returns>
        [HttpPost]
        [ProducesResponseType(typeof(AutomationLog), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] AutomationLog request)
        {
            if (request == null)
            {
                ModelState.AddModelError("Save", "No data passed");
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
    }
}
