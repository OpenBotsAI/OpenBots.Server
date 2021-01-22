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
        private readonly CredentialManager manager;
        private readonly Credential validCredential;
        private readonly Credential invalidCredential;

        public CredentialManagerTests()
        {
            //arrange
            var options = new DbContextOptionsBuilder<StorageContext>()
                .UseInMemoryDatabase(databaseName: "CredentialManager")
                .Options;

            var context = new StorageContext(options);
            validCredential = new Credential
            {
                Id = new Guid("10ea9a48-7365-4b86-8897-e1d5969137e6"),
                StartDate = new DateTime(2000, 12, 31, 12, 00, 0),
                EndDate = new DateTime(3000, 12, 31, 12, 00, 0),
            };

            invalidCredential = new Credential
            {
                Id = new Guid("10ea9a48-7365-4b86-8897-e1d5969137e6"),
                StartDate = new DateTime(2000, 12, 31, 12, 00, 0),
                EndDate = new DateTime(2010, 12, 31, 12, 00, 0),
            };

            var logger = Mock.Of<ILogger<Credential>>();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(It.IsAny<string>());

            var repo = new CredentialRepository(context, logger, httpContextAccessor.Object);
            manager = new CredentialManager(repo);
        }

        //validates if the current date falls within date range
        [Fact]
        public async Task ValidateRetreivalDate()
        {
            

            //act
            var validDateRange = manager.ValidateRetrievalDate(validCredential);
            var invalidDateRange = manager.ValidateRetrievalDate(invalidCredential);

            //assert
            Assert.True(validDateRange);
            Assert.False(invalidDateRange);
        }

        //validates if the end date is greater than start date
        [Fact]
        public async Task ValidateStartAndEndDates()
        {
            //act

            //end date is greater than start date
            validCredential.StartDate = new DateTime(2020, 12, 31, 12, 00, 0);
            validCredential.EndDate = new DateTime(2022, 12, 31, 12, 00, 0);

            //start date is greater than end date
            invalidCredential.StartDate = new DateTime(2020, 12, 31, 12, 00, 0);
            invalidCredential.EndDate = new DateTime(2010, 12, 31, 12, 00, 0);

            var validDateValues = manager.ValidateStartAndEndDates(validCredential);
            var invalidDateValues = manager.ValidateStartAndEndDates(invalidCredential);

            //assert
            Assert.True(validDateValues);
            Assert.False(invalidDateValues);
        }
    }
}
