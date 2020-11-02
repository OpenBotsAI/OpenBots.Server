using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel
{
    public class OrganizationListing : INameIDPair
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public bool? IsAdministrator { get; set; }
    }
}
