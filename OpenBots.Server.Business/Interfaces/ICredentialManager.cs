using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;

namespace OpenBots.Server.Business
{
    public interface ICredentialManager : IManager
    {
        bool ValidateRetrievalDate(Credential credential);
        bool ValidateStartAndEndDates(Credential credential);
        Credential CreateGlobalCredential(GlobalCredentialViewModel request);
        Credential CreateAgentCredential(AgentCredentialViewModel request);
        Credential DeleteCredential(string id);
        void CredentialNameAvailability(Credential request);
        Credential GetMatchingCredential(string credentialName);
    }
}

