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
    public class CredentialManagerTests
    {
        [Fact]
        public async Task ValidateRetreivalDate()
        {
            // arrange
            var options = new DbContextOptionsBuilder<StorageContext>()
                .UseInMemoryDatabase(databaseName: "ValidateDate")
                .Options;

            var context = new StorageContext(options);
            var validCredential = new Credential
            {
                Id = new Guid("10ea9a48-7365-4b86-8897-e1d5969137e6"),
                StartDate = new DateTime(2000, 12, 31, 12, 00, 0),
                EndDate = new DateTime(3000, 12, 31, 12, 00, 0),
            };

            var invalidCredential = new Credential
            {
                Id = new Guid("10ea9a48-7365-4b86-8897-e1d5969137e6"),
                StartDate = new DateTime(2000, 12, 31, 12, 00, 0),
                EndDate = new DateTime(2010, 12, 31, 12, 00, 0),
            };

            var logger = Mock.Of<ILogger<Credential>>();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(It.IsAny<string>());

            var repo = new CredentialRepository(context, logger, httpContextAccessor.Object);
            var manager = new CredentialManager(repo);

            // act
            var validDateRange = manager.ValidateRetrievalDate(validCredential);
            var invalidDateRange = manager.ValidateRetrievalDate(invalidCredential);

            // assert
            Assert.True(validDateRange);
            Assert.False(invalidDateRange);
        }
    }
}
