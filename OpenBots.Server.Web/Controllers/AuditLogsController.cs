﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using OpenBots.Server.WebAPI.Controllers;
using OpenBots.Server.Business;
using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.AuditLog;
using OpenBots.Server.Model.Attributes;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.Web.Controllers
{
    /// <summary>
    /// Controller for audit logs
    /// </summary>
    [V1]
    [Route("api/v{apiVersion:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class AuditLogsController : EntityController<AuditLog>
    {
        private readonly IAuditLogManager manager;
        private IConfiguration config { get; }
        private readonly IAuditLogRepository repository;

        /// <summary>
        /// AuditLogController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="membershipManager"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="manager"></param>
        /// <param name="configuration"></param>
        public AuditLogsController(
            IAuditLogRepository repository,
            IMembershipManager membershipManager,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IAuditLogManager manager) : base(repository, userManager, httpContextAccessor, membershipManager, configuration)
        {
            this.manager = manager;
            config = configuration;
            this.repository = repository;
        }

        /// <summary>
        /// Provides a list of all audit logs
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <param name="orderBy"></param>
        /// <response code="200">Ok, a paginated list of all audit logs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated list of all audit logs</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<AuditLogViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<AuditLogViewModel> Get(
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0
        )
        {
            return base.GetMany<AuditLogViewModel>();
        }

        /// <summary>
        /// Provides a ViewModel list of all audit logs
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="top"></param>
        /// <param name="skip"></param>
        /// <param name="orderBy"></param>
        /// <response code="200">Ok, a paginated list of all audit logs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Paginated ViewModel list of all audit logs</returns>
        [HttpGet("view")]
        [ProducesResponseType(typeof(PaginatedList<AuditLogViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public PaginatedList<AuditLogViewModel> GetView(
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 100,
        [FromQuery(Name = "$skip")] int skip = 0
        )
        {
            ODataHelper<AuditLogViewModel> oData = new ODataHelper<AuditLogViewModel>();

            string queryString = "";

            if (HttpContext != null
                && HttpContext.Request != null
                && HttpContext.Request.QueryString != null
                && HttpContext.Request.QueryString.HasValue)
                queryString = HttpContext.Request.QueryString.Value;

            oData.Parse(queryString);
            Guid parentguid = Guid.Empty;
            var newNode = oData.ParseOrderByQuery(queryString);
            if (newNode == null)
                newNode = new OrderByNode<AuditLogViewModel>();

            Predicate<AuditLogViewModel> predicate = null;
            if (oData != null && oData.Filter != null)
                predicate = new Predicate<AuditLogViewModel>(oData.Filter);
            int take = (oData?.Top == null || oData?.Top == 0) ? 100 : oData.Top;

            return manager.GetAuditLogsView(predicate, newNode.PropertyName, newNode.Direction, oData.Skip, take);
        }

        /// <summary>
        /// Gets count of AuditLogs in database
        /// </summary>
        /// <param name="filter"></param>
        /// <response code="200">Ok, count of all audit logs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Count of all audit logs</returns>
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
        /// Provides additional details on a specific audit Log
        /// </summary>
        /// <param name="id">Audit log id</param>
        /// <response code="200">Ok, if an audit log exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if audit log id is not in proper format</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no audit Log exists for the given audit log id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Audit log details for the given id</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PaginatedList<AuditLog>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            try
            {
                return await base.GetEntity(id.ToString());
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides additional ViewModel details on a specific audit Log
        /// </summary>
        /// <param name="id">Audit log id</param>
        /// <response code="200">Ok, if an audit log exists with the given id</response>
        /// <response code="304">Not modified</response>
        /// <response code="400">Bad request, if audit log id is not in proper format</response>
        /// <response code="403">Forbidden</response>
        /// <response code="404">Not found, when no audit Log exists for the given audit log id</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Audit log details for the given id</returns>
        [HttpGet("{id}/view")]
        [ProducesResponseType(typeof(PaginatedList<AuditLog>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status304NotModified)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetDetailsView(Guid id)
        {
            try
            {
                var log = repository.GetOne(id);

                string name = repository.GetServiceName(log);

                var logView = new AuditLogDetailsViewModel();
                logView = logView.Map(log);
                logView.ServiceName = name;

                PaginatedList<AuditLogDetailsViewModel> logList = new PaginatedList<AuditLogDetailsViewModel>();
                logList.Add(logView);

                return Ok(logList);
            }
            catch (Exception ex)
            {
                return ex.GetActionResult();
            }
        }

        /// <summary>
        /// Provides a list of all audit logs by name
        /// </summary>
        /// <response code="200">Ok, a list of all audit bogs</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="404">Not found</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>List of all audit logs</returns>
        [HttpGet("AuditLogsLookup")]
        [ProducesResponseType(typeof(List<AuditLogsLookupViewModel>), StatusCodes.Status200OK)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public AuditLogsLookupViewModel AllAuditLogs()
        {
            var response = repository.Find(null, x => x.IsDeleted == false);
            AuditLogsLookupViewModel auditLogsList = new AuditLogsLookupViewModel();

            if (response != null)
            {
                auditLogsList.ServiceNameList = new List<string>();       
                foreach (AuditLog item in response.Items)
                {
                    string serviceName = item.ServiceName;
                    string name = repository.GetServiceName(item);

                    if (!auditLogsList.ServiceNameList.Contains(name))
                        auditLogsList.ServiceNameList.Add(name);
                }
            }
            return auditLogsList;
        }

        /// <summary>
        /// Exports audit logs into a downloadable file
        /// </summary>
        /// <param name="fileType">Specifies the file type to be downloaded: csv, zip or, json</param>
        /// <param name="orderBy"></param>
        /// <param name="skip"></param>
        /// <param name="top"></param>
        /// <param name="filter"></param>
        /// <response code="200">Ok, if a log exists with the given filters</response>
        /// <response code="400">Bad request</response>
        /// <response code="403">Forbidden, unauthorized access</response>
        /// <response code="422">Unprocessable entity</response>
        /// <returns>Downloadable file</returns>
        [HttpGet("export/{filetype?}")]
        [Produces("text/csv", "application/zip", "application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public async Task<object> Export(
        [FromQuery(Name = "$filter")] string filter = "",
        [FromQuery(Name = "$orderby")] string orderBy = "",
        [FromQuery(Name = "$top")] int top = 0,
        [FromQuery(Name = "$skip")] int skip = 0, string fileType = "")
        {
            try
            {
                //Determine top value
                int maxExport = int.Parse(config["App:MaxExportRecords"]);
                top = top > maxExport | top == 0 ? maxExport : top; //If $top is greater than max or equal to 0 use maxExport value
                ODataHelper<AuditLog> oData = new ODataHelper<AuditLog>();
                string queryString = HttpContext.Request.QueryString.Value;

                oData.Parse(queryString);
                oData.Top = top;

                var auditLogsJson = base.GetMany(oData: oData);
                string csvString = manager.GetAuditLogs(auditLogsJson.Items.ToArray());
                var csvFile = File(new System.Text.UTF8Encoding().GetBytes(csvString), "text/csv", "AuditLogs.csv");

                switch (fileType.ToLower())
                {
                    case "csv":
                        return csvFile;

                    case "zip":
                        var zippedFile = manager.ZipCsv(csvFile);
                        const string contentType = "application/zip";
                        HttpContext.Response.ContentType = contentType;
                        var zipFile = new FileContentResult(zippedFile.ToArray(), contentType)
                        {
                            FileDownloadName = "AuditLogs.zip"
                        };

                        return zipFile;

                    case "json":
                        return auditLogsJson;
                }
                return csvFile;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Export", ex.Message);
                return ex.GetActionResult();
            }
        }
    }
}
