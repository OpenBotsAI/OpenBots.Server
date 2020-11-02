using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBots.Server.Web
{
    public class AppVersionHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            string version = string.Empty;
            version = string.Concat(version, " ", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            return Task.FromResult(HealthCheckResult.Healthy(version.Trim()));
        }
    }
}
