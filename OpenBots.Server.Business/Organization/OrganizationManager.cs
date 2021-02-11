using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Model.Membership;
using System.Linq;

namespace OpenBots.Server.Business
{
    public class OrganizationManager : BaseManager, IOrganizationManager
    {
        readonly IOrganizationRepository _organizationRepository;
        readonly IOrganizationUnitRepository _organizationUnitRepository;

        public OrganizationManager(
             IOrganizationRepository organizationRepository,
             IOrganizationUnitRepository organizationUnitRepository)
        {
            _organizationRepository = organizationRepository;
            _organizationUnitRepository = organizationUnitRepository;
        }

        public override void SetContext(UserSecurityContext userSecurityContext)
        {
            _organizationRepository.SetContext(userSecurityContext);
            _organizationUnitRepository.SetContext(userSecurityContext);
            base.SetContext(userSecurityContext);
        }

        public Organization AddNewOrganization(Organization value)
        {
            var organization = _organizationRepository.Add(value);
            if (organization != null)
            {
                OrganizationUnit orgUnit = new OrganizationUnit()
                {
                    OrganizationId = organization.Id,
                    Name = "Common",
                    IsVisibleToAllOrganizationMembers = true,
                    CanDelete = false
                };
                _organizationUnitRepository.ForceIgnoreSecurity();
                _organizationUnitRepository.Add(orgUnit);
            }

            return organization;
        }

        public Organization GetDefaultOrganization()
        {
            var organization = _organizationRepository.Find(null, o => true)?.Items?.FirstOrDefault();
            
            return organization;
        }
    }
}
