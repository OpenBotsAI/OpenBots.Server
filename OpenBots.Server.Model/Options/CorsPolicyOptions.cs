namespace OpenBots.Server.Model.Options
{
    public class CorsPolicyOptions
    {
        public const string Origins = "Origins";

        public string AllowedOrigins { get; set; }
        public string ExposedHeaders { get; set; }
        public string PolicyName { get; set; }
    }
}
