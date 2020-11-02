namespace OpenBots.Server.Model.Options
{
    public class AppInsightOptions
    {
        public const string ApplicationInsights = "ApplicationInsights";

        public string InstrumentationKey { get; set; }
        public bool IsEnabled { get; set; }
    }
}
