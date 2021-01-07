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

        public bool ValidateRetrievalDate(Credential credential) //Ensure Current Date falls within Start-End Date range
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

        public bool ValidateStartAndEndDates(Credential credential) //Validate Start and EndDate Values
        {
            if (credential.StartDate == null || credential.EndDate == null) //Valid if either wasn't provided
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
