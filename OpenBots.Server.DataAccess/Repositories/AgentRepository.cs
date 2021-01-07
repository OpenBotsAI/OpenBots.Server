﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using System;
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

        public Agent FindAgent(string machineName, string macAddress, string ipAddress, Guid? agentID)
        {
            var agent = DbTable().Where(AuthorizeRead()).AsQueryable().Where(RemoveSoftDeleted())
                                                                      .Where(a => a.Id == agentID
                                                                               && a.MachineName.Equals(machineName) 
                                                                               && a.MacAddresses.Equals(macAddress) 
                                                                               && a.IPAddresses.Equals(ipAddress)).FirstOrDefault();
            return agent;
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
                                    IPAddresses = a.IPAddresses,
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
