using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using System;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTests
{
    public class AutomationLogManagerTests
    {
        [Fact]
        public async Task GetJobLogs()
        {
            //arrange
            var options = new DbContextOptionsBuilder<StorageContext>()
                .UseInMemoryDatabase(databaseName: "ValidatesId")
                .Options;

            var context = new StorageContext(options);
            AutomationLog[] processLogArray = new AutomationLog[10];

            for (int i = 0; i < 10; i++)
            {
                var dummyProcessLog = new AutomationLog
                {
                    Id = Guid.NewGuid(),
                    JobId = Guid.NewGuid()
                };
                Seed(context, dummyProcessLog);
                processLogArray[i] = dummyProcessLog;
            }

            var logger = Mock.Of<ILogger<AutomationLog>>();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(It.IsAny<string>());

            var repo = new AutomationLogRepository(context, logger, httpContextAccessor.Object);
            var manager = new AutomationLogManager(repo);

            //act
            var validCsv = manager.GetJobLogs(processLogArray);

            var lineCount = validCsv.Split('\n').Length;

            //assert
            Assert.Equal(11, lineCount);
        }

        private void Seed(StorageContext context, AutomationLog model)
        {
            var items = new[]
            {
                model
            };

            context.AutomationLogs.AddRange(items);
            context.SaveChanges();
        }
    }
}
