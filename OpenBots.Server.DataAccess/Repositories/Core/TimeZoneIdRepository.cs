using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class TimeZoneIdRepository : ReadOnlyEntityRepository<TimeZoneId>, ITimeZoneIdRepository
    {
        public TimeZoneIdRepository(StorageContext context, ILogger<TimeZoneId> logger) : base(context, logger)
        {
        }

        protected override Microsoft.EntityFrameworkCore.DbSet<TimeZoneId> DbTable()
        {
            return DbContext.TimeZoneIds;
        }
    }
}
