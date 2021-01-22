using OpenBots.Server.Model.Core;

namespace OpenBots.Server.Model.Configuration
{
    public class ConfigurationValue : NamedEntity
    {
        public const string Values = "Values";
        public new string Name { get; set; }
        public string Value { get; set; }
        public string? Description { get; set; }
        public string? UIHint { get; set; }
        public string? ValidationRegex { get; set; }
    }
}
