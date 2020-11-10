namespace OpenBots.Server.Model.Options
{
    /// <summary>
    /// Options for Cross-Origin Resource Sharing(CORS) Configuration
    /// </summary>
    /// <remarks>
    /// CORS policy allows a server to explicitly allow some cross-origin requests while rejecting
    /// others.
    /// </remarks>
    public class CorsPolicyOptions
    {
        /// <summary>
        /// Configuration Name in App Settings For CorsPolicyOtions
        /// </summary>
        public const string Origins = "Origins";

        /// <summary>
        /// List of origin URLs that are permitted to make requests in the current policy. If none
        /// are provided, then all origins will be allowed
        /// </summary>
        /// <remarks>
        /// Multiple origins can be specified by seperating them with a semicolon in appsettings
        /// </remarks>
        public string AllowedOrigins { get; set; }

        /// <summary>
        /// List of Headers that are exposed to the application in addition to the default headers
        /// </summary>
        /// <remarks>
        /// Multiple headers can be specified by seperating them with a semicolon in appsettings
        /// </remarks>
        public string ExposedHeaders { get; set; }

        /// <summary>
        /// Specifies the name of the policy to be created
        /// </summary>
        public string PolicyName { get; set; }
    }
}
