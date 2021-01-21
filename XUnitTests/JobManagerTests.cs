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
            //arrange
            var options = new DbContextOptionsBuilder<StorageContext>()
                .UseInMemoryDatabase(databaseName: "JobManager")
                .Options;
            StorageContext context = new StorageContext(options);

            newJobAgentId = Guid.NewGuid();
            completedJobAgentId = Guid.NewGuid();
            newJobId = Guid.NewGuid();
            
            //job with status of new
            Job newDummyJob = new Job 
            {
                Id = Guid.NewGuid(),
                JobStatus = JobStatusType.New,
                AgentId = newJobAgentId,
                CreatedOn = DateTime.UtcNow
            };

            //job with status of completed
            Job completedDummyJob = new Job
            {
                Id = Guid.NewGuid(),
                JobStatus = JobStatusType.Completed,
                AgentId = completedJobAgentId,
                CreatedOn = DateTime.UtcNow
            };

            //job Parameter to be removed
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

            //populate in memory database
            Seed(context, jobsToAdd, jobParameter);

            //create loggers
            var jobLogger = Mock.Of<ILogger<Job>>();
            var agentLogger = Mock.Of<ILogger<Agent>>();
            var processLogger = Mock.Of<ILogger<Automation>>();
            var jobParameterLogger = Mock.Of<ILogger<JobParameter>>();
            var jobCheckpointLogger = Mock.Of<ILogger<JobCheckpoint>>();

            //context accessor
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(It.IsAny<string>());

            //instance of necessary repositories
            var jobRepository = new JobRepository(context, jobLogger, httpContextAccessor.Object);
            var agentRepo = new AgentRepository(context, agentLogger, httpContextAccessor.Object);
            var automationRepo = new AutomationRepository(context, processLogger, httpContextAccessor.Object);
            var jobParameterRepo = new JobParameterRepository(context, jobParameterLogger, httpContextAccessor.Object);
            var jobCheckpointRepo = new JobCheckpointRepository(context, jobCheckpointLogger, httpContextAccessor.Object);

            //manager to be tested
            manager = new JobManager(jobRepository, agentRepo, automationRepo, jobParameterRepo, jobCheckpointRepo);
        }

        //gets the next job that has not been picked up for the specified agent id
        [Fact]
        public async Task GetNextJob()
        {
            //act
            var jobsAvailable = manager.GetNextJob(newJobAgentId);
            var jobsCompleted  = manager.GetNextJob(completedJobAgentId);

            //assert
            Assert.True(jobsAvailable.IsJobAvailable); //agent id with a new job
            Assert.False(jobsCompleted.IsJobAvailable); //agent id without a new job
        }

        //get the job parameters for the specified job id
        [Fact]
        public async Task GetJobParameters()
        {
            //act
            var jobParameters = manager.GetJobParameters(newJobId);

            //assert
            Assert.Equal(jobParameter,jobParameters.First());
        }

        //delete job parameters for the specified job id
        [Fact]
        public async Task DeleteExistingParameters()
        {
            //act
            manager.DeleteExistingParameters(newJobId);
            var jobParameters = manager.GetJobParameters(newJobId);

            //assert
            Assert.Empty(jobParameters);
        }

        //used to seed the in-memory database
        private void Seed(StorageContext context, Job[] jobs, JobParameter jobParameter)
        {
            context.Jobs.AddRange(jobs);
            context.JobParameters.AddRange(jobParameter);
            context.SaveChanges();
        }
    }
}
