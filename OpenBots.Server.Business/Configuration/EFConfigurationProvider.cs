using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenBots.Server.DataAccess;
using OpenBots.Server.Model.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBots.Server.Business
{
    public class EFConfigurationProvider : ConfigurationProvider
    {
        public EFConfigurationProvider(Action<DbContextOptionsBuilder> optionsAction)
        {
            OptionsAction = optionsAction;
        }

        Action<DbContextOptionsBuilder> OptionsAction;

        public override void Load()
        {
            var builder = new DbContextOptionsBuilder<StorageContext>();

            OptionsAction(builder);

            using (var dbContext = new StorageContext(builder.Options))
            {
                dbContext.Database.EnsureCreated();

                Data = !dbContext.ConfigurationValues.Any()
                    ? CreateAndSaveDefaultValues(dbContext)
                    : dbContext.ConfigurationValues.ToDictionary(c => c.Name, c => c.Value);
            }
        }

        private static IDictionary<string, string> CreateAndSaveDefaultValues(StorageContext dbContext)
        {
            var configValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "BinaryObjects:Adapter", "FileSystemAdapter" },
                { "BinaryObjects:Path", "BinaryObjects"},
                { "Queue.Global:DefaultMaxRetryCount", "3" },
                { "App:EnableSwagger", "true"},
                { "App:MaxExportRecords", "100"},
                { "App:MaxReturnRecords", "100"},
            };

            dbContext.ConfigurationValues.AddRange(configValues
                .Select(kvp => new ConfigurationValue
                {
                    Name = kvp.Key,
                    Value = kvp.Value
                })
                .ToArray());

            dbContext.SaveChanges();

            return configValues;
        }
    }
}