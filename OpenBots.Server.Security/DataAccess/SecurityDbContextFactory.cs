using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Core;

namespace OpenBots.Server.Security.DataAccess
{
    public class SecurityDbContextFactory : IDesignTimeDbContextFactory<SecurityDbContext>
    {
        public const string ConnectionStringName = "Default";

        public SecurityDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SecurityDbContext>();

            var configuration = AppConfigurations.Get(
               WebContentDirectoryFinder.CalculateContentRootFolder());
            SecurityDbContextConfigurer.Configure(optionsBuilder, configuration.GetConnectionString("Sql"), configuration.GetValue<bool>("DbOption:UseSqlServer"));

            return new SecurityDbContext(optionsBuilder.Options);
        }
    }
}
