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

        public bool ValidateRetrievalDate(Credential credential)
        {
            if (DateTime.Now > credential.StartDate && DateTime.Now < credential.EndDate)
            {
                return true;
            }
            return false;
        }
    }
}
