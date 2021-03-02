using System;
using System.Collections.Generic;

namespace OpenBots.Server.Model.Core
{
    public class EmailAddress
    {
        public EmailAddress()
        {
        }

        public System.Net.Mail.MailAddress ToMailAddress()
        {
            return new System.Net.Mail.MailAddress(Address, Name);
        }

        public EmailAddress(System.Net.Mail.MailAddress address)
        {
            Name = address.DisplayName;
            Address = address.Address;
        }

        public static List<System.Net.Mail.MailAddress> IterateBack(IEnumerable<EmailAddress> addresses)
        {
            List<System.Net.Mail.MailAddress> addressStubs = new List<System.Net.Mail.MailAddress>();

            foreach (var addr in addresses)
            {
                addressStubs.Add(new System.Net.Mail.MailAddress(addr.Address, addr.Name));
            }

            return addressStubs;
        }

        public EmailAddress(string address)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }

        public EmailAddress(string name, string address)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }

        public string Name { get; set; }
        public string Address { get; set; }
    }
}
