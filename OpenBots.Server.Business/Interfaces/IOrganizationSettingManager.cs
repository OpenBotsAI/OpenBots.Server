using System;
using System.Collections.Generic;
using System.Text;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IOrganizationSettingManager : IManager
    {
        bool HasDisallowedExecution();
    }
}
