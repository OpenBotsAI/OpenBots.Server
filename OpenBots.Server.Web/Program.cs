using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenBots.Server.Business;

namespace OpenBots.Server.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureLogging(l => l.AddConsole().AddAzureWebAppDiagnostics().AddApplicationInsights())
            .ConfigureAppConfiguration((hostingContext, configBuilder) =>
            {
                var config = configBuilder.Build();
                var configSource = new EFConfigurationSource(
                    options => options.UseSqlServer(config.GetConnectionString("Sql")));
                configBuilder.Add(configSource);

                configBuilder.AddEnvironmentVariables(prefix: "OPENBOTS_");
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseIIS();
                webBuilder.UseStartup<Startup>();
               
            });
    }
}
