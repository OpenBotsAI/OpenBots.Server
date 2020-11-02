using System;

namespace OpenBots.Server.Model.Core
{
    public class EmailHeader
    {
        public EmailHeader()
        {
        }

        public EmailHeader(string name, string value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}
