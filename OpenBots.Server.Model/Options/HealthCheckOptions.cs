using System.Collections.Generic;

namespace OpenBots.Server.Model.Options
{
    /// <summary>
    /// Options for HealthCheckSetupOptions Configuration
    /// </summary>
    /// <remarks>
    /// HealthChecks let you validate if any external resource needed for your application is 
    /// working properly
    /// </remarks>
    public class HealthCheckSetupOptions
    {
        /// <summary>
        /// Configuration Name in App Settings For HealthChecks
        /// </summary>
        public const string HealthChecks = "HealthChecks";

        /// <summary>
        /// Used to enable health checks in the application
        /// </summary>
        public bool isEnabled { get; set; }

        /// <summary>
        /// Defines the relative path that will be used to check the health status of the application
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// References the HealthChecksUIOptions subsection in appsettings
        /// </summary>
        public HealthChecksUIOptions HealthChecksUI { get; set; }
    }

    /// <summary>
    /// Options for HealthChecksUIOptions Configuration
    /// </summary>
    /// <remarks>
    /// HealthChecksUI provides an interface for users to verify the status of their application
    /// </remarks>
    public class HealthChecksUIOptions
    {
        /// <summary>
        /// Configuration Name in App Settings For HealthChecksUI
        /// </summary>
        public const string HealthChecksUI = "HealthChecksUI";

        /// <summary>
        /// Used to enable the UI portion of the health checks
        /// </summary>
        public bool HealthChecksUIEnabled { get; set; }

        /// <summary>
        /// Defines the relative path that will be used to access the health checks UI
        /// </summary>
        public string UIRelativePath { get; set; }

        /// <summary>
        /// Defines the relative path that will be used to access the health checks api
        /// </summary>
        public string ApiRelativePath { get; set; }

        /// <summary>
        /// List which contains name and uri of status to be displayed in UI
        /// </summary>
        public List<string> HealthChecks { get; set; }

        /// <summary>
        /// Configures the number of seconds it takes for the UI to poll for healtchecks
        /// </summary>
        public int EvaluationTimeOnSeconds { get; set; }

        /// <summary>
        /// Sets minimum number of seconds between failure notifications to avoid receiver flooding
        /// </summary>
        public int MinimumSecondsBetweenFailureNotifications { get; set; }
    }
}
