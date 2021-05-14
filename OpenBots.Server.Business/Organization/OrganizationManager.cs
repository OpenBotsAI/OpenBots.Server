using Microsoft.Extensions.Configuration;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model.Identity;
using OpenBots.Server.Model.Membership;
using System;
using System.Linq;

namespace OpenBots.Server.Business
{
    public class OrganizationManager : BaseManager, IOrganizationManager
    {
        readonly IOrganizationRepository _organizationRepository;
        readonly IOrganizationUnitRepository _organizationUnitRepository;
        private IConfiguration _config { get; }


        public OrganizationManager(
             IOrganizationRepository organizationRepository,
             IOrganizationUnitRepository organizationUnitRepository,
             IConfiguration configuration)

        {
            _organizationRepository = organizationRepository;
            _organizationUnitRepository = organizationUnitRepository;
            _config = configuration;
        }

        public override void SetContext(UserSecurityContext userSecurityContext)
        {
            _organizationRepository.SetContext(userSecurityContext);
            _organizationUnitRepository.SetContext(userSecurityContext);
            base.SetContext(userSecurityContext);
        }

        public Organization AddNewOrganization(Organization value)
        {
            value.MaxStorageInBytes = long.Parse(_config["Organization:MaxStorageInBytes"]);
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

        public long? GetMaxStorageInBytes(Guid? organizationId = null)
        {
            Guid? entityId;
            if (organizationId == null || organizationId == Guid.Empty)
                entityId = GetDefaultOrganization()?.Id ?? Guid.Empty;
            else entityId = organizationId;

            Organization currentOrganization = _organizationRepository.GetOne(entityId.Value);

            long? maxOrgStorage = currentOrganization?.MaxStorageInBytes;
            long? defaultMaxStorage = long.Parse(_config["Organization:MaxStorageInBytes"]);

            if (maxOrgStorage != null && maxOrgStorage > 0)
                return maxOrgStorage.Value;
            else
                return defaultMaxStorage ?? long.MaxValue;
        }
    }
}
