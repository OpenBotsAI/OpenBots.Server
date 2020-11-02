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
    public class JobManagerTests
    {
        [Fact]
        public async Task GetNextJob()
        {
            // arrange
            var options = new DbContextOptionsBuilder<StorageContext>()
                .UseInMemoryDatabase(databaseName: "NextJob")
                .Options;

            Guid newJobAgentId = Guid.NewGuid();
            Guid completedJobAgentId = Guid.NewGuid();
            var context = new StorageContext(options);

            var newDummyJob = new Job
            {
                Id = Guid.NewGuid(),
                JobStatus = JobStatusType.New,
                AgentId = newJobAgentId,
                CreatedOn = DateTime.Now
            };

            var completedDummyJob = new Job
            {
                Id = Guid.NewGuid(),
                JobStatus = JobStatusType.Completed,
                AgentId = completedJobAgentId,
                CreatedOn = DateTime.Now
            };

            Seed(context, newDummyJob);
            Seed(context, completedDummyJob);

            var jobLogger = Mock.Of<ILogger<Job>>();
            var agentLogger = Mock.Of<ILogger<AgentModel>>();
            var processLogger = Mock.Of<ILogger<Process>>();

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(It.IsAny<string>());

            var jobRepository = new JobRepository(context, jobLogger, httpContextAccessor.Object);
            var agentRepo = new AgentRepository(context, agentLogger, httpContextAccessor.Object);
            var processRepo = new ProcessRepository(context, processLogger, httpContextAccessor.Object);

            var manager = new JobManager(jobRepository, agentRepo, processRepo);

            // act
            var jobsAvailable = manager.GetNextJob(newJobAgentId);
            var jobsCompleted  = manager.GetNextJob(completedJobAgentId);

            // assert
            Assert.True(jobsAvailable.IsJobAvailable);
            Assert.False(jobsCompleted.IsJobAvailable);
        }

        private void Seed(StorageContext context, Job job)
        {
            var agents = new[]
            {
                job
            };

            context.Jobs.AddRange(job);
            context.SaveChanges();
        }
    }
}
