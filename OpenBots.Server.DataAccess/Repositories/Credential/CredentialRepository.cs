using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class CredentialRepository : EntityRepository<Credential>, ICredentialRepository
    {
        public CredentialRepository(StorageContext context, ILogger<Credential> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<Credential> DbTable()
        {
            return dbContext.Credentials;
        }

        public PaginatedList<CredentialViewModel> FindAllView(Predicate<CredentialViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<CredentialViewModel> paginatedList = new PaginatedList<CredentialViewModel>();
            paginatedList.TotalCount = 0;

            var itemsList = base.Find(null, j => j.IsDeleted == false);
            if (itemsList != null && itemsList.Items != null && itemsList.Items.Count > 0)
            {
                var itemRecord = from c in itemsList.Items
                                 join a in dbContext.Agents on c.AgentId equals a.Id into table1
                                 from a in table1.DefaultIfEmpty()
                                 select new CredentialViewModel
                                 {
                                     Id = c.Id,
                                     Name = c.Name,
                                     Provider = c.Provider,
                                     StartDate = c.StartDate,
                                     EndDate = c.EndDate,
                                     Domain = c.Domain,
                                     UserName = c.UserName,
                                     PasswordHash = c.PasswordHash,
                                     Certificate = c.Certificate,
                                     AgentId = c.AgentId,
                                     AgentName = a?.Name
                                 };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        itemRecord = itemRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        itemRecord = itemRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                List<CredentialViewModel> filterRecord = null;
                if (predicate != null)
                    filterRecord = itemRecord.ToList().FindAll(predicate);
                else
                    filterRecord = itemRecord.ToList();

                paginatedList.Items = filterRecord.Skip(skip).Take(take).ToList();

                paginatedList.Completed = itemsList.Completed;
                paginatedList.Impediments = itemsList.Impediments;
                paginatedList.PageNumber = itemsList.PageNumber;
                paginatedList.PageSize = itemsList.PageSize;
                paginatedList.ParentId = itemsList.ParentId;
                paginatedList.Started = itemsList.Started;
                paginatedList.TotalCount = filterRecord?.Count;
            }

            return paginatedList;
        }
    }
}
