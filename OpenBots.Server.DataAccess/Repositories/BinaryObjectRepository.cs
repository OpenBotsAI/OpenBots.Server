using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpenBots.Server.DataAccess.Repositories
{
    /// <summary>
    /// Binary Object Repository
    /// </summary>
    public class BinaryObjectRepository : EntityRepository<BinaryObject>, IBinaryObjectRepository
    {
        private readonly ClaimsPrincipal _caller;

        /// <summary>
        /// Constructor for Binary Object Repository
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        /// <param name="entityEventSink"></param>
        /// <param name="httpContextAccessor"></param>
        public BinaryObjectRepository(StorageContext context, ILogger<BinaryObject> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
            _caller = httpContextAccessor.HttpContext.User;
        }

        /// <summary>
        /// Access Binary Objects Data Table
        /// </summary>
        /// <returns>Binary Object</returns>
        protected override DbSet<BinaryObject> DbTable()
        {
            return dbContext.BinaryObjects;
        }

        /// <summary>
        /// Update an Entity to the Binary Objects data table
        /// </summary>
        /// <param name="file"></param>
        /// <param name="filePath"></param>
        /// <param name="binaryObjectId"></param>
        /// <param name="apiComponent"></param>
        /// <param name="name"></param>
        /// <returns>Nothing</returns>
        public async Task UpdateEntity(IFormFile file, string filePath, string binaryObjectId, string apiComponent, string folder, string name, string hash)
        {
            BinaryObject entity = await DbTable().FirstOrDefaultAsync(x => x.Id == Guid.Parse(binaryObjectId));

            if (entity != null)
            {
                //Save properties as entity in BinaryObjects table in Server
                entity.ContentType = file.ContentType;
                entity.UpdatedBy = _caller.Identity.Name;
                entity.UpdatedOn = DateTime.UtcNow;
                entity.HashCode = hash;
                entity.Name = name;
                entity.SizeInBytes = file.Length;
                entity.Folder = folder;
                entity.StoragePath = filePath;

                Update(entity);
            }
        }
    }
}