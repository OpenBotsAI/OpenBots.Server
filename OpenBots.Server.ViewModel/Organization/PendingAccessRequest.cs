using System;

namespace OpenBots.Server.ViewModel
{
    public class PendingAccessRequest
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
