using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.Configuration;
using System;

namespace OpenBots.Server.ViewModel
{
    public class EmailAccountViewModel : IViewModel<EmailAccount, EmailAccountViewModel>
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsDefault { get; set; }
        public string Provider { get; set; }
        public bool IsSslEnabled { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string ApiKey { get; set; }
        public string FromEmailAddress { get; set; }
        public string FromName { get; set; }
        public DateTime StartOnUTC { get; set; }
        public DateTime EndOnUTC { get; set; }

        public EmailAccountViewModel Map(EmailAccount entity)
        {
            EmailAccountViewModel emailAccountViewModel = new EmailAccountViewModel();

            emailAccountViewModel.Id = entity.Id;
            emailAccountViewModel.Name = entity.Name;
            emailAccountViewModel.IsDisabled = entity.IsDisabled;
            emailAccountViewModel.IsDefault = entity.IsDefault;
            emailAccountViewModel.Provider = entity.Provider;
            emailAccountViewModel.IsSslEnabled = entity.IsSslEnabled;
            emailAccountViewModel.Host = entity.Host;
            emailAccountViewModel.Port = entity.Port;
            emailAccountViewModel.Username = entity.Username;
            emailAccountViewModel.PasswordHash = entity.PasswordHash;
            emailAccountViewModel.ApiKey = entity.ApiKey;
            emailAccountViewModel.FromEmailAddress = entity.FromEmailAddress;
            emailAccountViewModel.FromName = entity.FromName;
            emailAccountViewModel.StartOnUTC = entity.StartOnUTC;
            emailAccountViewModel.EndOnUTC = entity.EndOnUTC;

            return emailAccountViewModel;
        }
    }

    public class EmailAccountLookup
    {
        public Guid EmailAccountId { get; set; }
        public string EmailAccountName { get; set; }
    }
}
