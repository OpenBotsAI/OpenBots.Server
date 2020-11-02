using OpenBots.Server.Model.Identity;

namespace OpenBots.Server.Business
{
    public interface IManager
    {
        void SetContext(UserSecurityContext userSecurityContext);
    }
}