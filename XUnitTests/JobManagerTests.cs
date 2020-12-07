using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTests
{
    public class JobManagerTests
    {
        private readonly JobManager manager;
        private readonly JobParameter jobParameter;
        private readonly Guid newJobAgentId;
        private readonly Guid completedJobAgentId;
        private readonly Guid newJobId;

        public JobManagerTests()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<StorageContext>()
                .UseInMemoryDatabase(databaseName: "JobManager")
                .Options;
            StorageContext context = new StorageContext(options);

            newJobAgentId = Guid.NewGuid();
            completedJobAgentId = Guid.NewGuid();
            newJobId = Guid.NewGuid();
            
            //Job with status of new
            Job newDummyJob = new Job 
            {
                Id = Guid.NewGuid(),
                JobStatus = JobStatusType.New,
                AgentId = newJobAgentId,
                CreatedOn = DateTime.Now
            };

            // Job with status of completed
            Job completedDummyJob = new Job
            {
                Id = Guid.NewGuid(),
                JobStatus = JobStatusType.Completed,
                AgentId = completedJobAgentId,
                CreatedOn = DateTime.Now
            };

            // Job Parameter to be removed
            jobParameter = new JobParameter
            {
                Id = Guid.NewGuid(),
                DataType = "text",
                Value = "Sample Value",
                JobId = newJobId
            };

            Job[] jobsToAdd = new[]
            {
                newDummyJob,
                completedDummyJob
            };

            //Populate in memory database
            Seed(context, jobsToAdd, jobParameter);

            //Create loggers
            var jobLogger = Mock.Of<ILogger<Job>>();
            var agentLogger = Mock.Of<ILogger<AgentModel>>();
            var processLogger = Mock.Of<ILogger<Automation>>();
            var jobParameterLogger = Mock.Of<ILogger<JobParameter>>();
            var jobCheckpointLogger = Mock.Of<ILogger<JobCheckpoint>>();

            // Context accessor
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(It.IsAny<string>());

            // Instance of necessary repositories
            var jobRepository = new JobRepository(context, jobLogger, httpContextAccessor.Object);
            var agentRepo = new AgentRepository(context, agentLogger, httpContextAccessor.Object);
            var processRepo = new AutomationRepository(context, processLogger, httpContextAccessor.Object);
            var jobParameterRepo = new JobParameterRepository(context, jobParameterLogger, httpContextAccessor.Object);
            var jobCheckpointRepo = new JobCheckpointRepository(context, jobCheckpointLogger, httpContextAccessor.Object);

            //manager to be tested
            manager = new JobManager(jobRepository, agentRepo, processRepo, jobParameterRepo, jobCheckpointRepo);
        }

        // Gets the next job that has not been picked up for the specified agent ID
        [Fact]
        public async Task GetNextJob()
        {
            // act
            var jobsAvailable = manager.GetNextJob(newJobAgentId);
            var jobsCompleted  = manager.GetNextJob(completedJobAgentId);

            // assert
            Assert.True(jobsAvailable.IsJobAvailable); //Agent ID with a new Job
            Assert.False(jobsCompleted.IsJobAvailable); // Agent ID without a new JobS
        }

        //Get the Job Parameters for the specified Job ID
        [Fact]
        public async Task GetJobParameters()
        {
            // act
            var jobParameters = manager.GetJobParameters(newJobId);

            // assert
            Assert.Equal(jobParameter,jobParameters.First());
        }

        //Delete Job Parameters for the specified Job ID
        [Fact]
        public async Task DeleteExistingParameters()
        {
            // act
            manager.DeleteExistingParameters(newJobId);
            var jobParameters = manager.GetJobParameters(newJobId);

            // assert
            Assert.Empty(jobParameters);
        }

        //Used to seed the InMemory Database
        private void Seed(StorageContext context, Job[] jobs, JobParameter jobParameter)
        {
            context.Jobs.AddRange(jobs);
            context.JobParameters.AddRange(jobParameter);
            context.SaveChanges();
        }
    }
}
