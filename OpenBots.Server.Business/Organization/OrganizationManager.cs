using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Model.Membership;
using System.Linq;

namespace OpenBots.Server.Business
{
    public class OrganizationManager : BaseManager, IOrganizationManager
    {
        readonly IOrganizationRepository organizationRepository;
        readonly IOrganizationUnitRepository organizationUnitRepository;

        public OrganizationManager(
             IOrganizationRepository organizationRepository,
             IOrganizationUnitRepository organizationUnitRepository)
        {
            this.organizationRepository = organizationRepository;
            this.organizationUnitRepository = organizationUnitRepository;
        }

        public override void SetContext(UserSecurityContext userSecurityContext)
        {
            organizationRepository.SetContext(userSecurityContext);
            organizationUnitRepository.SetContext(userSecurityContext);
            base.SetContext(userSecurityContext);
        }

        public Organization AddNewOrganization(Organization value)
        {
            var organization = organizationRepository.Add(value);
            if (organization != null)
            {
                OrganizationUnit orgUnit = new OrganizationUnit()
                {
                    OrganizationId = organization.Id,
                    Name = "Common",
                    IsVisibleToAllOrganizationMembers = true,
                    CanDelete = false
                };
                organizationUnitRepository.ForceIgnoreSecurity();
                organizationUnitRepository.Add(orgUnit);
            }

            return organization;
        }

        public Organization GetDefaultOrganization()
        {
            var organization = organizationRepository.Find(null, o => true)?.Items?.FirstOrDefault();
            
            return organization;
        }
    }
}
