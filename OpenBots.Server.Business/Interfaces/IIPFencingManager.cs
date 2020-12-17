using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Membership;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.Net;

namespace OpenBots.Server.Business
{
    public interface IIPFencingManager : IManager
    {
        bool MatchedOnRule(IPAddress iPAddress, List<IPFencing> ipFencingRules, IHeaderDictionary headers);
        bool IsRequestAllowed(IPAddress iPAddress, IPFencingMode? fencingMode = null);
        IPFencingMode? GetIPFencingMode(Guid organizationId);
    }
}
