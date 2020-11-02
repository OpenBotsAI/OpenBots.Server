using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Model.Membership;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenBots.Server.DataAccess.Exceptions;

namespace OpenBots.Server.Business
{
    public class MembershipManager : BaseManager, IMembershipManager
    {

        protected IPersonRepository _personRepo;
        protected IOrganizationRepository _organizationRepo;
        protected IOrganizationMemberRepository _organzationMemberRepo;
        protected IAccessRequestRepository _accessRequestRepo;
        protected IPersonEmailRepository _personEmailRepository;
        protected IEmailVerificationRepository _emailVerificationRepository;
        protected IAccessRequestsManager _accessRequestManager;
        protected IOrganizationUnitRepository _organizationUnitRepository;
        protected IAspNetUsersRepository _aspNetUsersRepository;

        public MembershipManager(
            IPersonRepository personRepo,
            IOrganizationRepository organizationRepo,
            IOrganizationMemberRepository organzationMemberRepo,
            IAccessRequestRepository accessRequestRepo,
            IPersonEmailRepository personEmailRepository,
            IEmailVerificationRepository emailVerificationRepository,
            IAccessRequestsManager accessRequestManager,
            IOrganizationUnitRepository organizationUnitRepository,
            IAspNetUsersRepository aspNetUsersRepository)
        {
            _personRepo = personRepo;
            _organizationRepo = organizationRepo;
            _organzationMemberRepo = organzationMemberRepo;
            _accessRequestRepo = accessRequestRepo;
            _personEmailRepository = personEmailRepository;
            _emailVerificationRepository = emailVerificationRepository;
            _accessRequestManager = accessRequestManager;
            _organizationUnitRepository = organizationUnitRepository;
            _aspNetUsersRepository = aspNetUsersRepository;
        }

        public override void SetContext(UserSecurityContext userSecurityContext)
        {

            _personRepo.SetContext(userSecurityContext); ;
            _organizationRepo.SetContext(userSecurityContext); ;
            _organzationMemberRepo.SetContext(userSecurityContext); ;
            _accessRequestRepo.SetContext(userSecurityContext);
            _personEmailRepository.SetContext(userSecurityContext);
            _emailVerificationRepository.SetContext(userSecurityContext);
            _accessRequestManager.SetContext(userSecurityContext);
            _organizationUnitRepository.SetContext(userSecurityContext);
            base.SetContext(userSecurityContext);
        }

        public PaginatedList<TeamMemberViewModel> GetPeopleInOrganization(Guid organizationId, string sortColumn, OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            PaginatedList<TeamMemberViewModel> team = new PaginatedList<TeamMemberViewModel>
            {
                ParentId = organizationId
            };

            var org = _organizationRepo.GetOne(organizationId);

            if (org == null)
                throw new KeyNotFoundException();

            bool orgHasEmailDomain = false;
            string emailDomainWithAt = string.Empty;

            var members = _organzationMemberRepo.Find(organizationId);
            team.TotalCount = members.Items.Count;

            var personIds = members.Items.Select(om => om.PersonId).Distinct().ToArray();

            var people = _personRepo.Find(null, p => personIds.Contains(p.Id));
            var emails = _emailVerificationRepository.Find(null, e => personIds.Contains(e.PersonId));

            foreach (OrganizationMember member in members.Items.Skip(skip).Take(take))
            {
                TeamMemberViewModel teamMember = new TeamMemberViewModel
                {
                    OrganizationMemberId = member.Id.Value,
                    PersonId = member.PersonId.Value,
                    JoinedOn = member.CreatedOn.Value,
                    InvitedBy = member.InvitedBy,
                    IsAdmin = member.IsAdministrator.GetValueOrDefault(false)
                };

                var person = people.Items.Where(p => p.Id.Equals(member.PersonId)).FirstOrDefault();
                if(person != null)
                {
                    teamMember.UserName = person.Name;
                    teamMember.Name = person.Name;
                    teamMember.Status = "InActive";
                    if (orgHasEmailDomain)
                    {
                        var matchingEmailList = emails.Items.Where(e => e.PersonId.Equals(member.PersonId) && e.Address.EndsWith(emailDomainWithAt)).ToList();
                        var orderedList = matchingEmailList.OrderBy(x => x.CreatedOn);
                        var correctEmailForMember = orderedList.FirstOrDefault();

                        if (correctEmailForMember != null)
                        {
                            teamMember.EmailAddress = correctEmailForMember.Address;
                            teamMember.Status = correctEmailForMember.IsVerified.GetValueOrDefault(false) ? "Active" : "InActive";
                        }
                    }
                    if(string.IsNullOrEmpty(teamMember.EmailAddress))
                    {
                        var matchingEmailList = emails.Items.Where(e => e.PersonId.Equals(member.PersonId)).ToList();
                        var orderedList = matchingEmailList.OrderBy(x => x.CreatedOn);
                        var correctEmailForMember = orderedList.FirstOrDefault();

                        if (correctEmailForMember != null)
                        {
                            teamMember.EmailAddress = correctEmailForMember.Address;
                            teamMember.Status = correctEmailForMember.IsVerified.GetValueOrDefault(false) ? "Active" : "InActive";
                        }
                    }
                }

                team.Add(teamMember);
            }

            if (!string.IsNullOrWhiteSpace(sortColumn))
                if (direction == OrderByDirectionType.Ascending)
                    team.Items = team.Items.OrderBy(t => t.GetType().GetProperty(sortColumn).GetValue(t)).ToList();
                else if (direction == OrderByDirectionType.Descending)
                    team.Items = team.Items.OrderByDescending(t => t.GetType().GetProperty(sortColumn).GetValue(t)).ToList();
            
            return team;
        }

        public PaginatedList<OrganizationMember> GetOrganizationMember(Guid organizationId, Guid personId)
        {
            var members = _organzationMemberRepo.Find(organizationId, p => p.PersonId == personId);
            return members;
        }

        public PaginatedList<OrganizationCard> MyOrganizations(Guid personId, bool IncludeAccessRequestedOrg = false)
        {
            _organzationMemberRepo.ForceIgnoreSecurity();
            var orgMems = _organzationMemberRepo.Find(null, om => om.PersonId.Equals(personId));
            var orgIds = orgMems.Items.Select(om => om.OrganizationId).Distinct().ToArray();
            var orgs = _organizationRepo.Find(null, o => orgIds.Contains(o.Id));

            _organzationMemberRepo.ForceSecurity();


            PaginatedList<OrganizationCard> cards = new PaginatedList<OrganizationCard>();
            foreach (Organization org in orgs.Items)
            {
                OrganizationCard card = new OrganizationCard
                {
                    Id = org.Id.Value,
                    Name = org.Name,
                    CreatedOn = org.CreatedOn,
                    CanLeave = true
                };
                var orgMem = orgMems.Items.Where(om => om.OrganizationId == org.Id).FirstOrDefault();
                if (orgMem != null)
                {
                    card.JoinedOn = orgMem.CreatedOn;
                    card.IsOrganizationMember = true;
                    if (orgMem.IsAdministrator.HasValue && orgMem.IsAdministrator.Value)
                        card.CanDelete = true;
                }
                Guid creatorPersonId = Guid.Empty;
                if (Guid.TryParse(org.CreatedBy, out creatorPersonId))
                {
                    Person creatorPerson = _personRepo.GetOne(creatorPersonId);
                    if (creatorPerson != null)
                    {
                        card.CreatedBy = string.Format("{0}", creatorPerson.Name);
                    }
                }
                if (string.IsNullOrEmpty(card.CreatedBy))
                    card.CreatedBy = org.CreatedBy;


                cards.Add(card);
            }

            if (IncludeAccessRequestedOrg) {
                //Organizations requested for access
                var accessRequest = PendingOrganizationAccess(personId);
                if (accessRequest?.Items?.Count > 0) cards.Items.AddRange(accessRequest.Items);
            }

            cards.ParentId = personId;
            cards.TotalCount = cards.Items.Count;
            return cards;
        }

        public PaginatedList<OrganizationCard> PendingOrganizationAccess(Guid personId) {

            PaginatedList<OrganizationCard> cards = new PaginatedList<OrganizationCard>();
            _accessRequestRepo.ForceIgnoreSecurity();
            var accessRequests = _accessRequestRepo.Find(null, ar => ar.IsAccessRequested.HasValue && ar.IsAccessRequested.Value && ar.PersonId == personId, ar => ar.AccessRequestedOn, OrderByDirectionType.Descending);
            _accessRequestRepo.ForceSecurity();
            var orgIds = accessRequests.Items.Select(om => om.OrganizationId).Distinct().ToArray();

            if (orgIds.Length > 0)
            {
                var orgs = _organizationRepo.Find(null, o => orgIds.Contains(o.Id), o => o.Name, OrderByDirectionType.Ascending);

                foreach (Organization org in orgs.Items)
                {
                    OrganizationCard card = new OrganizationCard
                    {
                        Id = org.Id.Value,
                        Name = org.Name,
                        CreatedOn = org.CreatedOn,
                        IsOrganizationMember = false
                    };
                    Guid creatorPersonId = Guid.Empty;
                    if (Guid.TryParse(org.CreatedBy, out creatorPersonId))
                    {
                        Person creatorPerson = _personRepo.GetOne(creatorPersonId);
                        if (creatorPerson != null)
                        {
                            card.CreatedBy = string.Format("{0}", creatorPerson.Name);
                        }
                    }
                    if (string.IsNullOrEmpty(card.CreatedBy))
                        card.CreatedBy = org.CreatedBy;

                    cards.Add(card);
                }
            }
            
            return cards;
        }

        public PaginatedList<OrganizationListing> Suggestions(Guid personId)
        {

            var emails = _personEmailRepository.Find(personId);

            List<Organization> org = new List<Organization>();
            foreach (PersonEmail email in emails.Items)
            {
                // Ensure Email Address is valid with a @ sign
                if (email != null && !string.IsNullOrEmpty(email.Address) && email.Address.Contains("@", StringComparison.InvariantCultureIgnoreCase))
                {
                    string emailDomain = email.Address.Split('@')[1];
                    
                    var orgMems = _organzationMemberRepo.Find(null, om => om.PersonId.Equals(personId));
                    var orgIds = orgMems.Items.Select(om => om.OrganizationId).Distinct().ToArray();

                    var orgs = _organizationRepo.Find(null, o => !orgIds.Contains(o.Id));
                    org.AddRange(orgs.Items);
                }
            }
            var listOfOrg = new PaginatedList<OrganizationListing>(org.Select(o => new OrganizationListing() { Id = o.Id, Name = o.Name }))
            {
                ParentId = personId
            };

            return listOfOrg;
        }

        public PaginatedList<OrganizationListing> Search(Guid personId, string startsWith, int skip, int take, bool isOrgMember = true)
        {
            PaginatedList<OrganizationMember> orgMems = null;
            _organzationMemberRepo.ForceIgnoreSecurity();
                // if (isOrgMember)
                orgMems = _organzationMemberRepo.Find(null, om => om.PersonId.Equals(personId));
            //else
              //  orgMems = _organzationMemberRepo.Find(null, om => !om.PersonId.Equals(personId));

            var orgIds = orgMems.Items.Select(om => om.OrganizationId).Distinct().ToArray();

            PaginatedList<Organization> orgs = new PaginatedList<Organization>();
            if (!isOrgMember)
            {
                orgs = _organizationRepo.Find(null, o => o.Name.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase) && !orgIds.Contains(o.Id), o => o.Name, OrderByDirectionType.Ascending, skip, take);
            }
            else {
                orgs = _organizationRepo.Find(null, o => o.Name.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase) && orgIds.Contains(o.Id), o => o.Name, OrderByDirectionType.Ascending, skip, take);
            }
            var listings = orgs.Items.Select(o => new OrganizationListing() { Id = o.Id, Name = o.Name, IsAdministrator = orgMems?.Items?.Find(p=>p.OrganizationId == o.Id.GetValueOrDefault())?.IsAdministrator.GetValueOrDefault(false)  });

            return new PaginatedList<OrganizationListing>(listings);
        }

        public PaginatedList<Person> GetEmailByName(Guid organizationId, string startsWith, int skip, int take, bool isOrgMember = true)
        {
            PaginatedList<OrganizationMember> orgMems = null;
            PaginatedList<Person> people = new PaginatedList<Person>();

            _organzationMemberRepo.ForceIgnoreSecurity();

            orgMems = _organzationMemberRepo.Find(null, om => om.OrganizationId.Equals(organizationId));

            foreach (var member in orgMems?.Items)
            {
                var person = _personRepo.Find(null, p => p.Id.Equals(member.PersonId))?.Items.FirstOrDefault();
                //var emails = _personEmailRepository.Find(null, pem => pem.PersonId.Equals(person.Id))?.Items;
                var user = _aspNetUsersRepository.Find(null, pem => pem.PersonId.Equals(person.Id))?.Items.FirstOrDefault();
                person.OrgEmail = user.Email;
                people?.Items.Add(person);
            }

            var matches = people.Items.FindAll(p => p.Name.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase));

            people.Items.Clear();

            foreach (var match in matches)
            {
                people.Items.Add(match);
            }
            
            return people;
        }

        public AccessRequest JoinOrganization(Guid personId, Guid organizationId)
        {
            _accessRequestRepo.ForceIgnoreSecurity();
            //Check if there is already access request pending before adding new
            var accessRequests = _accessRequestRepo.Find(null, ar => ar.PersonId == personId && ar.OrganizationId == organizationId && ar.IsAccessRequested.HasValue && ar.IsAccessRequested.Value)?.Items?.FirstOrDefault();

            if (accessRequests == null)
            {
                AccessRequest request = new AccessRequest
                {
                    AccessRequestedOn = DateTime.UtcNow,
                    PersonId = personId,
                    OrganizationId = organizationId,
                    IsAccessRequested = true
                };
                return _accessRequestRepo.Add(request);
            } else 
                {                
                    throw new CannotInsertDuplicateConstraintException(null, "Request", "", "Request"); ;
                }
            
        }

        public PaginatedList<PendingAccessRequest> Pending(Guid organizationId)
        {
            PaginatedList<PendingAccessRequest> pending = new PaginatedList<PendingAccessRequest>();
            var accessRequests = _accessRequestRepo.Find(organizationId, ar => ar.IsAccessRequested.HasValue && ar.IsAccessRequested.Value, ar => ar.AccessRequestedOn, OrderByDirectionType.Descending);
            foreach(AccessRequest request in accessRequests.Items)
            {
                if (request.Id.HasValue && request.PersonId.HasValue)
                {
                    PendingAccessRequest par = new PendingAccessRequest
                    {
                        Id = request.Id
                    };
                    var person = _personRepo.GetOne(request.PersonId.Value);
                    par.Name = person.Name;

                    var email = _personEmailRepository.Find(null, p => p.PersonId == request.PersonId.Value && p.IsPrimaryEmail == true)?.Items?.FirstOrDefault()?.Address;

                    if (email == null) 
                    {
                        email = _emailVerificationRepository.Find(null, ev => ev.PersonId == request.PersonId.Value)?.Items?.FirstOrDefault()?.Address;
                    }
                    par.Email = email;
                    pending.Add(par);
                }
            }
            pending.TotalCount = pending.Items.Count;
            pending.ParentId = organizationId;

            return pending;
        }

        public void ApproveAccessRequest(Guid personId, Guid organizationId, UserSecurityContext context, ApprovalDecisionType approvalActionType)
        {
            AccessRequest request = _accessRequestRepo.GetPendingAccessRequest(personId, organizationId);
            ApproveAccessRequest(request, context, approvalActionType);
        }

        public void RejectAccessRequest(Guid accessRequestId, UserSecurityContext context)
        {
            AccessRequest request = _accessRequestRepo.GetOne(accessRequestId);
            ApproveAccessRequest(request, context, ApprovalDecisionType.Reject);
        }

        public void ApproveAccessRequest(Guid accessRequestId, UserSecurityContext context, ApprovalDecisionType approvalActionType = ApprovalDecisionType.Approve)
        {
            AccessRequest request = _accessRequestRepo.GetOne(accessRequestId);
            ApproveAccessRequest(request, context, approvalActionType);
        }

        public void ApproveAccessRequest(AccessRequest accessRequest, UserSecurityContext context, ApprovalDecisionType approvalActionType)
        {
            Person approver = _personRepo.GetOne(context.PersonId);

            if (approver == null)
                throw new UnauthorizedAccessException("Approver not found");


            // Check if Person has Authority to Approve/Reject
            OrganizationMember approverMembership = _organzationMemberRepo.GetMember(approver.Id.Value, accessRequest.OrganizationId.Value);
            if (approverMembership != null && approverMembership.IsAdministrator.HasValue && approverMembership.IsAdministrator.Value)
            {
                accessRequest.IsAccessRequested = false;
                _accessRequestRepo.Update(accessRequest);

                if (approvalActionType == ApprovalDecisionType.Approve)
                {
                    _organzationMemberRepo.Approve(accessRequest.PersonId.Value, accessRequest.OrganizationId.Value, approver.Id.Value);
                }
            }
            else
            {
                throw new UnauthorizedAccessException("User is not an Administrator of this Organization");
            }

        }

        /// <summary>
        /// Invite users to organization
        /// </summary>
        /// <param name="inviteUser"></param>
        public OrganizationMember InviteUser(InviteUserViewModel inviteUser, UserSecurityContext context) 
        {
            OrganizationMember orgMember = null;

            //User in the system.. check email verification table
            var emailAddress = _emailVerificationRepository.Find(null, p => p.Address.Equals(inviteUser.Email, StringComparison.OrdinalIgnoreCase)).Items?.FirstOrDefault();
            if (emailAddress != null)
            {
                //Check if the person exists in the organization
                orgMember = _organzationMemberRepo.Find(null, p => p.PersonId == emailAddress.PersonId && p.OrganizationId == inviteUser.OrganizationId).Items?.FirstOrDefault();
                if (orgMember != null) throw new CannotInsertDuplicateConstraintException(null, "Organization Member", "", "Member");

                //Add to person to organization only if you are admin or add it to access request table
                var member = _organzationMemberRepo.Find(null, a => a.PersonId == SecurityContext.PersonId && a.OrganizationId == inviteUser.OrganizationId)?.Items.FirstOrDefault();
                var IsOrgAdmin = member != null && member.IsAdministrator.GetValueOrDefault(false);

                if (IsOrgAdmin)
                {
                    //Add to person to organization
                    OrganizationMember newOrgMember = new OrganizationMember()
                    {
                        PersonId = emailAddress.PersonId,
                        OrganizationId = inviteUser.OrganizationId,
                        IsAutoApprovedByEmailAddress = true,
                        IsInvited = true
                    };
                    orgMember = _organzationMemberRepo.Add(newOrgMember);

                }
                else
                {
                    //This will check if there is any access request pending 
                    var accessRequest = JoinOrganization(emailAddress.PersonId.GetValueOrDefault(), inviteUser.OrganizationId.GetValueOrDefault());

                    //Create dummy org member object to prevent getting it created as new user in the system
                    orgMember = new OrganizationMember()
                    {
                        PersonId = accessRequest.PersonId,
                        OrganizationId = accessRequest.OrganizationId,
                        IsInvited = true
                    };
                }
            }
            else { 
            }
            //if not send the invite on mail
            return orgMember;
        }

        public OrganizationMember RevokeAdminPermisson(Guid organizationId, Guid personId, UserSecurityContext context)
        {
            var orgMember = _organzationMemberRepo.Find(null, p => p.OrganizationId == organizationId && p.PersonId == personId && p.IsAdministrator.GetValueOrDefault(false)).Items?.FirstOrDefault();
            if (orgMember != null)
            {
                //check if you are admin to revoke the admin rights of other users 
                bool IsOrgAdmin = Array.Exists(context.OrganizationId, o => o.CompareTo(organizationId) == 0);
                if (IsOrgAdmin && context.PersonId == personId) return orgMember;

                if (IsOrgAdmin)
                {
                    orgMember.IsAdministrator = false;
                    orgMember = _organzationMemberRepo.Update(orgMember);
                }
            }
            return orgMember;
        }

        public OrganizationMember GrantAdminPermission(Guid organizationId, Guid personId) 
        {
            var orgMember = _organzationMemberRepo.Find(null, p => p.OrganizationId == organizationId && p.PersonId == personId && !p.IsAdministrator.GetValueOrDefault(false)).Items?.FirstOrDefault();
            if (orgMember != null)
            {
                orgMember.IsAdministrator = true;
                orgMember = _organzationMemberRepo.Update(orgMember);
            }
            return orgMember;
        }

        public PaginatedList<OrganizationUnit> GetDepartments(string id)
        {
            var departments = _organizationUnitRepository.Find(Guid.Parse(id));

            return departments;
        }
    }

    public enum ApprovalDecisionType
    {
        Unknown,
        Approve,
        Reject
    }
}
