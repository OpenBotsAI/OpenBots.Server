using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenBots.Server.ViewModel
{
    public class AgentCredentialViewModel : IViewModel<AgentCredentialViewModel, Credential>
    {
        [Required]
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Domain { get; set; }
        [Required]
        public string UserName { get; set; }
        [DoNotAudit]
        public string? PasswordSecret { get; set; }
        public string? PasswordHash { get; set; }
        public string? HashSalt { get; set; }
        public string? Certificate { get; set; }
        [Required]
        public Guid? AgentId { get; set; }

        public Credential Map(AgentCredentialViewModel? viewModel)
        {
            Credential credential = new Credential
            {
                Name = viewModel.Name,
                StartDate = viewModel.StartDate,
                EndDate = viewModel.EndDate,
                Domain = viewModel.Domain,
                UserName = viewModel.UserName,
                PasswordSecret = viewModel.PasswordSecret,
                PasswordHash = viewModel.PasswordHash,
                Certificate = viewModel.Certificate,
                AgentId = viewModel.AgentId
            };
            return credential;
        }
    }
}
