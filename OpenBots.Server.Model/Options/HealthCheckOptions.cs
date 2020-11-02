using System.Collections.Generic;

namespace OpenBots.Server.Model.Options
{
    public class HealthCheckSetupOptions
    {
        public const string HealthChecks = "HealthChecks";

        public bool isEnabled { get; set; }
        public string Endpoint { get; set; }
        public HealthChecksUIOptions HealthChecksUI { get; set; }
    }

    public class HealthChecksUIOptions
    {
        public const string HealthChecksUI = "HealthChecksUI";

        public bool HealthChecksUIEnabled { get; set; }
        public string UIRelativePath { get; set; }
        public string ApiRelativePath { get; set; }
        public List<string> HealthChecks { get; set; }
        public int EvaluationTimeOnSeconds { get; set; }
        public int MinimumSecondsBetweenFailureNotifications { get; set; }
    }
}
