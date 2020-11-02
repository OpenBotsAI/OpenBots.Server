using OpenBots.Server.Model.Membership;

namespace OpenBots.Server.Business
{
    public interface IOrganizationManager : IManager
    {
        Organization AddNewOrganization(Organization value);
        Organization GetDefaultOrganization();
    }
}