namespace OpenBots.Server.Model.Options
{
    /// <summary>
    /// Options for Tokens Configuration
    /// </summary>
    /// <remarks>
    /// Tokens allows us to authenticate any api request
    /// </remarks>
    public class TokensOptions
    {
        /// <summary>
        /// Configuration Name in App Settings For Tokens
        /// </summary>
        public const string Tokens = "Tokens";

        /// <summary>
        /// Specifies the key that will be used to validate the token issuer
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Sets the URI of the token issuer
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Sets the URI of the token audience
        /// </summary>
        public string Audience { get; set; }
    }
}
