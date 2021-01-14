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
        private readonly IOrganizationManager organizationManager;
        private readonly IOrganizationSettingRepository organizationSettingRepository;
        public OrganizationSettingManager(IOrganizationManager organizationManager,
                IOrganizationSettingRepository organizationSettingRepository)
        {
            this.organizationManager = organizationManager;
            this.organizationSettingRepository = organizationSettingRepository;
        }

        public bool HasDisallowedExecution()
        {
            var defaultOrganization = organizationManager.GetDefaultOrganization();

            organizationSettingRepository.ForceIgnoreSecurity();
            var orgSettings = organizationSettingRepository.Find(null, s => s.OrganizationId == defaultOrganization.Id).Items.FirstOrDefault();
            organizationSettingRepository.ForceSecurity();

            if (orgSettings != null && orgSettings.DisallowAllExecutions != null)
            {
                return orgSettings.DisallowAllExecutions;
            }
            return false;
        }
    }
}
