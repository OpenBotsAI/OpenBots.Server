using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using OpenBots.Server.ViewModel.AgentViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.DataAccess.Repositories
{
    public class AgentRepository : EntityRepository<Agent>, IAgentRepository
    {
        public AgentRepository(StorageContext context, ILogger<Agent> logger, IHttpContextAccessor httpContextAccessor) : base(context, logger, httpContextAccessor)
        {
        }

        protected override DbSet<Agent> DbTable()
        {
            return dbContext.Agents;
        }

        public PaginatedList<AllAgentsViewModel> FindAllView(Predicate<AllAgentsViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<AllAgentsViewModel> paginatedList = new PaginatedList<AllAgentsViewModel>();

            var itemsList = base.Find(null, a => a.IsDeleted == false);
            if (itemsList != null && itemsList.Items != null && itemsList.Items.Count > 0)
            {
                var itemRecord = from a in itemsList.Items
                                 join h in dbContext.AgentHeartbeats on a.Id equals h.AgentId into table1
                                 from h in table1.OrderByDescending(h=>h.CreatedOn).Take(1).DefaultIfEmpty()
                                 select new AllAgentsViewModel
                                 {
                                     Id = a?.Id,
                                     Name = a?.Name,
                                     MachineName = a?.MachineName,
                                     MacAddresses = a?.MacAddresses,
                                     IsEnabled = a.IsEnabled,
                                     LastReportedOn = h?.LastReportedOn,
                                     LastReportedStatus = h?.LastReportedStatus,
                                     LastReportedWork = h?.LastReportedWork,
                                     LastReportedMessage = h?.LastReportedMessage,
                                     IsHealthy = h?.IsHealthy,
                                     Status = a.IsConnected == false ? "Not Connected": ((DateTime)h?.LastReportedOn).AddMinutes(5) > DateTime.UtcNow ? "Connected": "Disconnected", 
                                     CredentialId = a?.CredentialId, 
                                     CreatedOn =  a?.CreatedOn
                                 };

                if (!string.IsNullOrWhiteSpace(sortColumn))
                    if (direction == OrderByDirectionType.Ascending)
                        itemRecord = itemRecord.OrderBy(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();
                    else if (direction == OrderByDirectionType.Descending)
                        itemRecord = itemRecord.OrderByDescending(j => j.GetType().GetProperty(sortColumn).GetValue(j)).ToList();

                List<AllAgentsViewModel> filterRecord = null;
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

        public AgentViewModel GetAgentDetailById(string id)
        {
            AgentViewModel agentViewModel = null;
            Guid agentId;
            Guid.TryParse(id, out agentId);

            var agent = base.Find(null, a => a.Id == agentId && a.IsDeleted == false);
            if(agent != null)
            {
                var agentView = from a in agent.Items
                                join c in dbContext.Credentials on a.CredentialId equals c.Id into table1
                                from c in table1.DefaultIfEmpty()
                                select new AgentViewModel
                                {
                                    Id = (a == null || a.Id == null) ? Guid.Empty : a.Id.Value,
                                    Name = a.Name,
                                    MachineName = a.MachineName,
                                    MacAddresses = a.MacAddresses,
                                    IsEnabled = a.IsEnabled,
                                    IsConnected = a.IsConnected,
                                    CredentialId = a.CredentialId,
                                    CredentialName = c?.Name
                                };

                agentViewModel = agentView.FirstOrDefault();
            }

            return agentViewModel;
        }
    }
}
