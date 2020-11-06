namespace OpenBots.Server.Model.Options
{
    /// <summary>
    /// Options for ApplicationInsights Configuration
    /// </summary>
    /// <remarks>
    /// Application Insights monitors request rates, response times, and failure rates
    /// </remarks>
    public class AppInsightOptions
    {
        /// <summary>
        /// Configuration Name in App Settings for AppInsightOptions
        /// </summary>
        public const string ApplicationInsights = "ApplicationInsights";

        /// <summary>
        /// Identifies the resource that you want to associate your telemetry data with
        /// </summary>
        public string InstrumentationKey { get; set; }

        /// <summary>
        /// Used to Enable Logs for Application Insights
        /// </summary>
        public bool IsEnabled { get; set; }
    }
}
