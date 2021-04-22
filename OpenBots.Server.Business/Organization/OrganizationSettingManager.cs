using OpenBots.Server.Business.Interfaces;
using OpenBots.Server.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenBots.Server.Business
{
    public class OrganizationSettingManager : BaseManager, IOrganizationSettingManager
    {
        private readonly IOrganizationManager _organizationManager;
        private readonly IOrganizationSettingRepository _organizationSettingRepository;
        public OrganizationSettingManager(IOrganizationManager organizationManager,
                IOrganizationSettingRepository organizationSettingRepository)
        {
            _organizationManager = organizationManager;
            _organizationSettingRepository = organizationSettingRepository;
        }

        public bool HasDisallowedExecution()
        {
            var defaultOrganization = _organizationManager.GetDefaultOrganization();

            _organizationSettingRepository.ForceIgnoreSecurity();
            var orgSettings = _organizationSettingRepository.Find(null, s => s.OrganizationId == defaultOrganization.Id).Items.FirstOrDefault();
            _organizationSettingRepository.ForceSecurity();

            if (orgSettings != null && orgSettings.DisallowAllExecutions != null)
            {
                return orgSettings.DisallowAllExecutions;
            }
            return false;
        }
    }
}
