using OpenBots.Server.Model.Core;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model
{
    public class Agent : NamedEntity
    {
        [Required]
        public string MachineName { get; set; }
        public string MacAddresses { get; set; }
        [Required]
        public bool IsEnabled { get; set; }
        [Required]
        public bool IsConnected { get; set; }
        public Guid? CredentialId { get; set; }
    }
}
