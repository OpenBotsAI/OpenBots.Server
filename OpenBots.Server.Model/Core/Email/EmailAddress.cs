using MimeKit;
using System;
using System.Collections.Generic;

namespace OpenBots.Server.Model.Core
{
    public class EmailAddress
    {
        public EmailAddress()
        {
        }

        public MailboxAddress ToMailAddress()
        {
            return new MailboxAddress(Name, Address);
        }

        public EmailAddress(MailboxAddress address)
        {
            Name = address.Name;
            Address = address.Address;
        }

        public static List<MailboxAddress> IterateBack(IEnumerable<EmailAddress> addresses)
        {
            List<MailboxAddress> addressStubs = new List<MailboxAddress>();

            foreach (var addr in addresses)
            {
                addressStubs.Add(new MailboxAddress(addr.Name, addr.Address));
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
