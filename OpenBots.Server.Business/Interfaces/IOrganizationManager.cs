using OpenBots.Server.Model.Membership;
using System;

namespace OpenBots.Server.Business
{
    public interface IOrganizationManager : IManager
    {
        Organization AddNewOrganization(Organization value);
        Organization GetDefaultOrganization();
        long? GetMaxStorageInBytes(Guid? organizationId = null);
    }
}