using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Model.Membership;
using System;

namespace OpenBots.Server.Business
{
    public class AccessRequestsManager : BaseManager, IAccessRequestsManager
    {
        private readonly IAccessRequestRepository _accessRequestRepo;
        private readonly IPersonRepository _personRepo;

        public AccessRequestsManager(IAccessRequestRepository accessRequestRepo, IPersonRepository personRepo)
        {
            _accessRequestRepo = accessRequestRepo;
            _personRepo = personRepo;
           
        }

        public override void SetContext(UserSecurityContext userSecurityContext)
        {
            _accessRequestRepo.SetContext(userSecurityContext);
            _personRepo.SetContext(userSecurityContext);
            base.SetContext(userSecurityContext);
        }

        public PaginatedList<AccessRequest> GetAccessRequests(string organizationId)
        {
            var accessRequests = _accessRequestRepo.Find(Guid.Parse(organizationId));

            foreach (AccessRequest accReqItem in accessRequests.Items)
            {
                var person = _personRepo.GetOne(accReqItem.PersonId.GetValueOrDefault());
                accReqItem.Person = person;
            }
            return accessRequests;
        }

        public AccessRequest AddAccessRequest(AccessRequest accessRequest)
        {
            var orgAccessRequest = _accessRequestRepo.Add(accessRequest);
            return orgAccessRequest;
        }

        public AccessRequest AddAnonymousAccessRequest(AccessRequest accessRequest)
        {
            _accessRequestRepo.ForceIgnoreSecurity();
            var orgAccessRequest = _accessRequestRepo.Add(accessRequest);
            _accessRequestRepo.ForceSecurity();
            return orgAccessRequest;
        }
    }
}
