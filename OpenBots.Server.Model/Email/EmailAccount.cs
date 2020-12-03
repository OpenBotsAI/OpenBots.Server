using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.Model.Configuration
{
    public class EmailAccount : NamedEntity
    {
        public bool IsDisabled { get; set; }
        public bool IsDefault { get; set; }
        public string Provider { get; set; }
        public bool IsSslEnabled { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        [DoNotAudit]
        public string EncryptedPassword { get; set; }
        public string PasswordHash { get; set; }
        public string ApiKey { get; set; }
        public string FromEmailAddress { get; set; }
        public string FromName { get; set; }
        public DateTime StartOnUTC { get; set; }
        public DateTime EndOnUTC { get; set; }
    }
}
