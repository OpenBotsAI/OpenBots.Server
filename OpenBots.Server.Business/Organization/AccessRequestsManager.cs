using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Model.Membership;
using System;

namespace OpenBots.Server.Business
{
    public class AccessRequestsManager : BaseManager, IAccessRequestsManager
    {
        private readonly IAccessRequestRepository accessRequestRepo;
        private readonly IPersonRepository personRepo;

        public AccessRequestsManager(IAccessRequestRepository accessRequestRepo, IPersonRepository personRepo)
        {
            this.accessRequestRepo = accessRequestRepo;
            this.personRepo = personRepo;
           
        }

        public override void SetContext(UserSecurityContext userSecurityContext)
        {
            this.accessRequestRepo.SetContext(userSecurityContext);
            this.personRepo.SetContext(userSecurityContext);
            base.SetContext(userSecurityContext);
        }

        public PaginatedList<AccessRequest> GetAccessRequests(string organizationId)
        {
            var accessRequests = accessRequestRepo.Find(Guid.Parse(organizationId));

            foreach (AccessRequest accReqItem in accessRequests.Items)
            {
                var person = personRepo.GetOne(accReqItem.PersonId.GetValueOrDefault());
                accReqItem.Person = person;
            }
            return accessRequests;
        }

        public AccessRequest AddAccessRequest(AccessRequest accessRequest)
        {
            var orgAccessRequest = accessRequestRepo.Add(accessRequest);
            return orgAccessRequest;
        }

        public AccessRequest AddAnonymousAccessRequest(AccessRequest accessRequest)
        {
            accessRequestRepo.ForceIgnoreSecurity();
            var orgAccessRequest = accessRequestRepo.Add(accessRequest);
            accessRequestRepo.ForceSecurity();
            return orgAccessRequest;
        }
    }
}
