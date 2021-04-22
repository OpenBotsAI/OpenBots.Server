using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model
{
    public class Credential : NamedEntity
    {
        public string? Provider { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Domain { get; set; }
        [Required]
        public string UserName { get; set; }
        [DoNotAudit]
        public string? PasswordSecret { get; set; }
        public string? PasswordHash { get; set; }
        public string? Certificate { get; set; }
    }

    public class CredentialsLookup
    {
        public Guid CredentialId { get; set; }
        public string CredentialName { get; set; }
    }
}

