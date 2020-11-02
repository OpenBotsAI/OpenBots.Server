using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Data;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using Serilog.Sinks.MSSqlServer;
using OpenBots.Server.Model.Attributes;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for logging Serilogs
    /// </summary>
    [V1]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class LoggerController : ControllerBase
    {
        private string _connectionString;

        /// <summary>
        /// LoggerController constructor
        /// </summary>
        /// <param name="configuration"></param>
        public LoggerController(IConfiguration configuration)
        {
            _connectionString = configuration["ConnectionStrings:Sql"];
        }

        /// <summary>
        /// Adds serilog logs from the agent to the process logs table
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Ok, all logs were stored correctly</response>
        /// <response code="400">Bad request, when the log value is not in proper format</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="409">Conflict, concurrency error</response> 
        /// <response code="422">Unprocessabile entity, when a duplicate record is being entered</response>
        /// <returns>Ok response if the logs were successfully stored</returns>
        [HttpPost("{logger?}")]
        [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Post([FromBody] dynamic request, string logger = "")
        {
            try
            {
                var columnOptions = new ColumnOptions
                {
                    AdditionalColumns = new Collection<SqlColumn>
                    {
                        new SqlColumn
                            {ColumnName = "JobId", PropertyName = "JobId", DataType = SqlDbType.UniqueIdentifier},
                        new SqlColumn
                            {ColumnName = "ProcessId",  PropertyName = "ProcessId", DataType = SqlDbType.UniqueIdentifier},
                         new SqlColumn
                            {ColumnName = "AgentId",  PropertyName = "AgentId", DataType = SqlDbType.UniqueIdentifier},
                        new SqlColumn
                            {ColumnName = "MachineName", PropertyName = "MachineName", DataType = SqlDbType.NVarChar},
                        new SqlColumn
                            {ColumnName = "AgentName", PropertyName = "AgentName", DataType = SqlDbType.NVarChar},
                        new SqlColumn
                            {ColumnName = "ProcessName", PropertyName = "ProcessName", DataType = SqlDbType.NVarChar},
                        new SqlColumn
                            {ColumnName = "Id",  PropertyName = "Id", DataType = SqlDbType.UniqueIdentifier},
                        new SqlColumn
                            {ColumnName = "ProcessLogTimeStamp",  PropertyName = "ProcessLogTimeStamp", DataType = SqlDbType.DateTime2},
                         new SqlColumn
                            {ColumnName = "CreatedOn",  PropertyName = "CreatedOn", DataType = SqlDbType.DateTime2},
                         new SqlColumn
                            {ColumnName = "Logger",  PropertyName = "Logger", DataType = SqlDbType.NVarChar},
                         new SqlColumn
                            {ColumnName = "IsDeleted",  PropertyName = "IsDeleted", DataType = SqlDbType.Bit},
                         new SqlColumn
                            {ColumnName = "Properties",  PropertyName = "Properties", DataType = SqlDbType.NVarChar}
                    }
                };

                columnOptions.Store.Remove(StandardColumn.TimeStamp);
                columnOptions.Store.Remove(StandardColumn.Id);
                columnOptions.Store.Remove(StandardColumn.Properties);

                var log = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo
                    .MSSqlServer(
                        connectionString: _connectionString,
                        sinkOptions: new SinkOptions { TableName = "ProcessLogs" ,AutoCreateSqlTable = true},
                        columnOptions: columnOptions)
                    .CreateLogger();

                var parser = new MessageTemplateParser();

                foreach (var serilog in request.events)
                {
                    var jobId = serilog.Properties.JobId == null ? (Guid?)null : new Guid(Convert.ToString(serilog.Properties.JobId));
                    var processId = serilog.Properties.ProcessId == null ? (Guid?)null : new Guid(Convert.ToString(serilog.Properties.ProcessId));
                    var agentId = serilog.Properties.AgentId == null ? (Guid?)null : new Guid(Convert.ToString(serilog.Properties.AgentId));

                    var id = Guid.NewGuid();
                    var timestamp = DateTimeOffset.Parse(serilog.Timestamp.ToString()).UtcDateTime;
                    var level = (LogEventLevel)TypeDescriptor.GetConverter(typeof(LogEventLevel)).ConvertFromString(serilog.Level.ToString());
                    var messageTemplate = parser.Parse(serilog.MessageTemplate.ToString());
                    var properties = new[] {
                                new LogEventProperty("JobId", new ScalarValue(jobId)),
                                new LogEventProperty("ProcessId", new ScalarValue(processId)),
                                new LogEventProperty("AgentId", new ScalarValue(agentId)),
                                new LogEventProperty("MachineName", new ScalarValue(serilog.Properties.MachineName.ToString())),
                                new LogEventProperty("AgentName", new ScalarValue(serilog.Properties.AgentName.ToString())),
                                new LogEventProperty("ProcessName", new ScalarValue(serilog.Properties.ProcessName.ToString())),
                                new LogEventProperty("ProcessLogTimeStamp", new ScalarValue(timestamp)),
                                new LogEventProperty("Id", new ScalarValue(id)),
                                new LogEventProperty("CreatedOn", new ScalarValue(DateTime.Now)),
                                new LogEventProperty("Logger", new ScalarValue(logger)),
                                new LogEventProperty("IsDeleted", new ScalarValue(false)),
                                new LogEventProperty("Properties", new ScalarValue(serilog.Properties)),
                        };

                    var logEvent = new LogEvent(timestamp, level, null, messageTemplate, properties);

                    log.Write(logEvent);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Logger", ex.Message);
                return BadRequest(ModelState);
            }
        }
    }
}
