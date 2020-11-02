using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace OpenBots.Server.Security.DataAccess
{

    public static class SecurityDbContextConfigurer
    {

        public static ILoggerFactory loggerFactory = LoggerFactory.Create(builder => {
            builder.AddDebug();
        });        

        public static void Configure(DbContextOptionsBuilder<SecurityDbContext> builder, string connectionString, bool useSqlServer)
        {
            if (useSqlServer)
            {
                builder.UseSqlServer(connectionString);
            }
            else
            {
                builder.UseSqlite(connectionString);
            }
            
             builder.UseLoggerFactory(loggerFactory);
        }

        public static void Configure(DbContextOptionsBuilder<SecurityDbContext> builder, DbConnection connection, bool useSqlServer)
        {
            if (useSqlServer)
            {
                builder.UseSqlServer(connection);
            }
            else
            {
                builder.UseSqlite(connection);
            }

             builder.UseLoggerFactory(loggerFactory);
        }
    }
}
