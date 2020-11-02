using System.ComponentModel.DataAnnotations;

namespace OpenBots.Server.Model.Configuration
{
    public class ConfigurationValue
    {
        public const string Values = "Values";

        [Key]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
