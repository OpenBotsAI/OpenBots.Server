using OpenBots.Server.Business;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using OpenBots.Server.DataAccess.Exceptions;
using Microsoft.Extensions.Configuration;

namespace OpenBots.Server.WebAPI.Controllers
{
    /// <summary>
    /// Controller for ReadOnlyEntity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ReadOnlyEntityController<T> : ApplicationBaseController
        where T : class, IEntity, new()
    {
        protected readonly IReadOnlyEntityRepository<T> readRepository;
        public IConfiguration config { get; }

        /// <summary>
        /// ReadOnlyEntityController constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userManager"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="membershipManager"></param>
        protected ReadOnlyEntityController(IReadOnlyEntityRepository<T> repository,
            ApplicationIdentityUserManager userManager,
            IHttpContextAccessor httpContextAccessor,
            IMembershipManager membershipManager,
            IConfiguration configuration) : base(httpContextAccessor, userManager, membershipManager)
        {
            readRepository = repository;
            config = configuration;
            readRepository.SetContext(SecurityContext);
        }

        /// <summary>
        /// Get paginated list of entities
        /// </summary>
        /// <param name="parentid"></param>
        /// <returns>Paginated list of entities</returns>
        /// <exception cref="EntityOperationException"></exception>
        /// <exception cref="EntityDoesNotExistException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        /// <exception cref="EntityValidationException"></exception>
        protected virtual PaginatedList<TViewModel> GetMany<TViewModel>(string parentid = "", ODataHelper<T> oData = null)
             where TViewModel : class, IViewModel<T, TViewModel>, new()
        {
            if (oData == null)
            {
                int maxRecords = int.Parse(config["App:MaxReturnRecords"]);
                oData = new ODataHelper<T>();
                string queryString = "";

                if (HttpContext != null
                    && HttpContext.Request != null
                    && HttpContext.Request.QueryString != null
                    && HttpContext.Request.QueryString.HasValue)
                    queryString = HttpContext.Request.QueryString.Value;

                oData.Parse(queryString);
                oData.Top = oData.Top == 0 ? maxRecords : oData.Top;
            }

            Guid parentguid = Guid.Empty;
            if (!string.IsNullOrEmpty(parentid))
                parentguid = new Guid(parentid);

            return readRepository.Find<TViewModel>(parentguid, oData.Filter, oData.Sort, oData.SortDirection, oData.Skip, oData.Top);
        }

        /// <summary>
        /// Get an individual entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parentid"></param>
        /// <returns>An individual entity</returns>
        /// <exception cref="EntityOperationException"></exception>
        /// <exception cref="EntityDoesNotExistException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        /// <exception cref="EntityValidationException"></exception>
        protected virtual async Task<IActionResult> GetEntity<TViewModel>(string id, string parentid = "")
            where TViewModel : class, IViewModel<T, TViewModel>, new()
        {
            T entity = null;
            TViewModel entityView = new TViewModel();

            entity = readRepository.GetOne(new Guid(id));

            if (entity == null)
            {
                ModelState.AddModelError("GetData", "Record does not exist or you do not have authorized access.");
                return BadRequest(ModelState);
            }
            else
            {
                entityView = entityView.Map(entity);
            }

            string timeStamp = "\"" + Convert.ToBase64String(entity.Timestamp) + "\"";
            if (Request.Headers.ContainsKey("if-none-match"))
            {
                string etag = Request.Headers["if-none-match"];

                if (!string.IsNullOrEmpty(etag) && etag.Equals(timeStamp, StringComparison.InvariantCultureIgnoreCase))
                {
                    return StatusCode(304); // Not Modified if ETag is same as Current Timestamp
                }
            }
            try
            {
                Response.Headers.Add("ETag", timeStamp);
            }
            catch
            {
                return Ok(entityView);
            }
            return Ok(entityView);
        }

        /// <summary>
        /// Uses ODataFilter to get a count of records in the database
        /// </summary>
        /// <param name="parentid"></param>
        /// <returns>Count of records in the database</returns>
        /// <exception cref="EntityOperationException"></exception>
        /// <exception cref="EntityDoesNotExistException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        /// <exception cref="EntityValidationException"></exception>
        protected virtual int? Count(string parentid = "")
        {
            ODataHelper<T> oData = new ODataHelper<T>();

            string queryString = "";

            if (HttpContext != null
                && HttpContext.Request != null
                && HttpContext.Request.QueryString != null
                && HttpContext.Request.QueryString.HasValue)
                queryString = HttpContext.Request.QueryString.Value;

            oData.Parse(queryString);
            Guid parentguid = Guid.Empty;
            if (!string.IsNullOrEmpty(parentid))
                parentguid = new Guid(parentid);

            var result = readRepository.Find(parentguid, oData.Filter, oData.Sort, oData.SortDirection, 0, 0);

            return result.TotalCount;
        }

        /// <summary>
        /// Gets all the entities
        /// </summary>
        /// <param name="parentid"></param>
        /// <returns>All the entities in the database</returns>
        /// <exception cref="EntityOperationException"></exception>
        /// <exception cref="EntityDoesNotExistException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        /// <exception cref="EntityValidationException"></exception>
        protected virtual PaginatedList<T> GetMany(string parentid = "", ODataHelper<T> oData = null)
        {
            if (oData == null)
            {
                int maxRecords = int.Parse(config["App:MaxReturnRecords"]);
                oData = new ODataHelper<T>();
                string queryString = "";

                if (HttpContext != null
                    && HttpContext.Request != null
                    && HttpContext.Request.QueryString != null
                    && HttpContext.Request.QueryString.HasValue)
                    queryString = HttpContext.Request.QueryString.Value;

                oData.Parse(queryString);
                oData.Top = oData.Top == 0 ? maxRecords : oData.Top;
            }           

            Guid parentguid = Guid.Empty;
            if (!string.IsNullOrEmpty(parentid))
                parentguid = new Guid(parentid);

            return readRepository.Find(parentguid, oData.Filter, oData.Sort, oData.SortDirection, oData.Skip, oData.Top);
        }

        /// <summary>
        /// Get an entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="parentid"></param>
        /// <returns>An entity from the database</returns>
        /// <exception cref="EntityOperationException"></exception>
        /// <exception cref="EntityDoesNotExistException"></exception>
        /// <exception cref="UnauthorizedOperationException"></exception>
        /// <exception cref="EntityValidationException"></exception>
        protected virtual async Task<IActionResult> GetEntity(string id, string parentid = "")
        {
            T entity = null;
            entity = readRepository.GetOne(new Guid(id));
            if (entity == null)
            {
                ModelState.AddModelError("GetData", "Record does not exist or you do not have authorized access.");
                return BadRequest(ModelState);
            }

            string timeStamp = "\"" + Convert.ToBase64String(entity.Timestamp) + "\"";
            if (Request.Headers.ContainsKey("if-none-match"))
            {
                string etag = Request.Headers["if-none-match"];

                if (!string.IsNullOrEmpty(etag) && etag.Equals(timeStamp, StringComparison.InvariantCultureIgnoreCase))
                {
                    return StatusCode(304); // Not Modified if ETag is same as Current Timestamp
                }
            }
            try
            {
                Response.Headers.Add("ETag", timeStamp);
            }
            catch 
            {
                return Ok(entity);
            }

            return Ok(entity);
        }
    }
}
