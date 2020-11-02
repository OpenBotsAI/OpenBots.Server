namespace OpenBots.Server.Model.Options
{
    public class TokensOptions
    {
        public const string Tokens = "Tokens";

        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
