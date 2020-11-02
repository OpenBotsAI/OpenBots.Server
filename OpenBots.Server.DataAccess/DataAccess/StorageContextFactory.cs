using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.Core;

namespace OpenBots.Server.DataAccess
{
    public class StorageContextFactory : IDesignTimeDbContextFactory<StorageContext>
    {
        public const string ConnectionStringName = "Default";
        public StorageContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<StorageContext>();

            var configuration = AppConfigurations.Get(
               WebContentDirectoryFinder.CalculateContentRootFolder()
           );
            StorageContextConfigurer.Configure(optionsBuilder, configuration.GetConnectionString("Sql"), configuration.GetValue<bool>("DbOption:UseSqlServer"));

            return new StorageContext(optionsBuilder.Options);
        }
    }
}
