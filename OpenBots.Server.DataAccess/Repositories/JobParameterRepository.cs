using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class JobParameterRepository : EntityRepository<JobParameter>, IJobParameterRepository
    {
        public JobParameterRepository(StorageContext context, ILogger<JobParameter> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<JobParameter> DbTable()
        {
            return dbContext.JobParameters;
        }
    }
}
