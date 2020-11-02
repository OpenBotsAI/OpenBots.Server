using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace OpenBots.Server.DataAccess
{

    public static class StorageContextConfigurer
    {

        private static ILoggerFactory loggerFactory = LoggerFactory.Create(builder => {
            builder.AddDebug();
        });

        public static void Configure(DbContextOptionsBuilder<StorageContext> builder, string connectionString, bool useSqlServer)
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

        public static void Configure(DbContextOptionsBuilder<StorageContext> builder, DbConnection connection, bool useSqlServer)
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
