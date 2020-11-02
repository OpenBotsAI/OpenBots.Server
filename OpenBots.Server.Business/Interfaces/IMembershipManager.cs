using System;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Model.Membership;
using OpenBots.Server.ViewModel;

namespace OpenBots.Server.Business
{
    public interface IMembershipManager : IManager
    {
        void ApproveAccessRequest(AccessRequest accessRequest, UserSecurityContext context, ApprovalDecisionType approvalActionType);
        void ApproveAccessRequest(Guid personId, Guid organizationId, UserSecurityContext context, ApprovalDecisionType approvalActionType);
        void ApproveAccessRequest(Guid accessRequestId, UserSecurityContext context, ApprovalDecisionType approvalActionType = ApprovalDecisionType.Approve);
        void RejectAccessRequest(Guid accessRequestId, UserSecurityContext context);

        PaginatedList<PendingAccessRequest> Pending(Guid organizationId);

        PaginatedList<OrganizationCard> MyOrganizations(Guid personId, bool IncludeAccessRequestedOrg = false);
        PaginatedList<OrganizationCard> PendingOrganizationAccess(Guid personId);

        AccessRequest JoinOrganization(Guid personId, Guid organizationId);

        PaginatedList<OrganizationListing> Search(Guid personId, string startsWith, int skip, int take, bool isOrgMember = true);

        PaginatedList<OrganizationListing> Suggestions(Guid personId);

        PaginatedList<TeamMemberViewModel> GetPeopleInOrganization(Guid organizationId, string sortColumn, OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        PaginatedList<OrganizationMember> GetOrganizationMember(Guid organizationId, Guid personId);

        OrganizationMember InviteUser(InviteUserViewModel inviteUser, UserSecurityContext context);

        OrganizationMember RevokeAdminPermisson(Guid organizationId, Guid personId, UserSecurityContext context);

        OrganizationMember GrantAdminPermission(Guid organizationId, Guid personId);
        PaginatedList<OrganizationUnit> GetDepartments(string organizationId);

        PaginatedList<Person> GetEmailByName(Guid organizationId, string startsWith, int skip, int take, bool isOrgMember = true);
    }
}