namespace OpenBots.Server.Model.Options
{
    /// <summary>
    /// Options for managaing feature flags
    /// </summary>
    /// <remarks> Feature flags allow us to enable/disable features using appSettings</remarks>
    public class FeatureManagementOptions
    {
        /// <summary>
        /// Configuration Name in App Settings For FeatureManagementOptions
        /// </summary>
        public const string FeatureManagement = "FeatureManagement";

        /// <summary>
        /// Used to enable the IPFencing feature in the application
        /// </summary>
        public bool IPFencing { get; set; }

        /// <summary>
        /// Used to enable the Swagger documentation feature in the application
        /// </summary>
        public bool Swagger { get; set; }

        /// <summary>
        /// Used to enable the Hangfire feature in the application
        /// </summary>
        public bool Hangfire { get; set; }

        /// <summary>
        /// Used to enable the HealthChecks-UI and HealthChecks-API 
        /// </summary>
        public bool HealthChecks { get; set; }

        /// <summary>
        /// Used to enable the Emails API 
        /// </summary>
        public bool Emails { get; set; }

        /// <summary>
        /// Used to enable the BinaryObjects API
        /// </summary>
        public bool Files { get; set; }

        /// <summary>
        /// Used to enable the Queues API
        /// </summary>
        public bool Queues { get; set; }
    }

    public enum MyFeatureFlags
    {
        IPFencing,
        Swagger,
        Hangfire,
        HealthChecks,
        Emails,
        Files,
        Queues
    }
}
