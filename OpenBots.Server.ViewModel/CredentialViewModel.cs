using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using System;

namespace OpenBots.Server.ViewModel
{
    public class CredentialViewModel : IViewModel<Credential, CredentialViewModel>
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string Provider { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string Certificate { get; set; }

        public CredentialViewModel Map(Credential entity)
        {
            CredentialViewModel credentialViewModel = new CredentialViewModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Provider = entity.Provider,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Domain = entity.Domain,
                UserName = entity.UserName,
                Certificate = entity.Certificate,
                PasswordHash = entity.PasswordHash
            };

            return credentialViewModel;
        }
    }
}
