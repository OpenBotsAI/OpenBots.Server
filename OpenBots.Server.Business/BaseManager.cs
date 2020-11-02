using OpenBots.Server.Model.Identity;

namespace OpenBots.Server.Business
{
    public class BaseManager : IManager
    {
        protected UserSecurityContext SecurityContext;

        public virtual void SetContext(UserSecurityContext userSecurityContext)
        {
            SecurityContext = userSecurityContext;
        }
    }
}
