using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using System;

namespace OpenBots.Server.Business
{
    public class CredentialManager : BaseManager, ICredentialManager
    {
        private readonly ICredentialRepository repo;

        public CredentialManager(ICredentialRepository repo)
        {
            this.repo = repo;
        }

        public bool ValidateRetrievalDate(Credential credential) //ensure current date falls within start-end date range
        {
            if (credential.StartDate != null)
            {
                if (DateTime.UtcNow < credential.StartDate)
                {
                    return false;
                }
            }

            if (credential.EndDate != null)
            {
                if (DateTime.UtcNow > credential.EndDate)
                {
                    return false;
                }
            }

            return true;
        }

        public bool ValidateStartAndEndDates(Credential credential) //validate start and end date values
        {
            if (credential.StartDate == null || credential.EndDate == null) //valid if either wasn't provided
            {
                return true;
            }
            if (credential.StartDate < credential.EndDate)
            {
                return true;
            }
            return false;
        }
    }
}
