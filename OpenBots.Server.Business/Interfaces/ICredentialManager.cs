using OpenBots.Server.Model;

namespace OpenBots.Server.Business
{
    public interface ICredentialManager : IManager
    {
        bool ValidateRetrievalDate(Credential credential);
    }
}

