using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Membership;

namespace OpenBots.Server.Business
{
    public interface IAccessRequestsManager : IManager
    {
        PaginatedList<AccessRequest> GetAccessRequests(string organizationId);
        AccessRequest AddAccessRequest(AccessRequest accessRequest);
        AccessRequest AddAnonymousAccessRequest(AccessRequest accessRequest);
    }
}