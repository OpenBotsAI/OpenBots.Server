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
        private readonly JobManager _manager;
        private readonly JobParameter _jobParameter;
        private readonly Guid _newJobAgentId;
        private readonly Guid _completedJobAgentId;
        private readonly Guid _newJobId;

        public JobManagerTests()
        {
            //arrange
            var options = new DbContextOptionsBuilder<StorageContext>()
                .UseInMemoryDatabase(databaseName: "JobManager")
                .Options;
            StorageContext context = new StorageContext(options);

            _newJobAgentId = Guid.NewGuid();
            _completedJobAgentId = Guid.NewGuid();
            _newJobId = Guid.NewGuid();
            
            //job with status of new
            Job newDummyJob = new Job 
            {
                Id = Guid.NewGuid(),
                JobStatus = JobStatusType.New,
                AgentId = _newJobAgentId,
                CreatedOn = DateTime.UtcNow
            };

            //job with status of completed
            Job completedDummyJob = new Job
            {
                Id = Guid.NewGuid(),
                JobStatus = JobStatusType.Completed,
                AgentId = _completedJobAgentId,
                CreatedOn = DateTime.UtcNow
            };

            //job Parameter to be removed
            _jobParameter = new JobParameter
            {
                Id = Guid.NewGuid(),
                DataType = "text",
                Value = "Sample Value",
                JobId = _newJobId
            };

            Job[] jobsToAdd = new[]
            {
                newDummyJob,
                completedDummyJob
            };

            //populate in memory database
            Seed(context, jobsToAdd, _jobParameter);

            //create loggers
            var jobLogger = Mock.Of<ILogger<Job>>();
            var agentLogger = Mock.Of<ILogger<Agent>>();
            var processLogger = Mock.Of<ILogger<Automation>>();
            var jobParameterLogger = Mock.Of<ILogger<JobParameter>>();
            var jobCheckpointLogger = Mock.Of<ILogger<JobCheckpoint>>();
            var automationVersionLogger = Mock.Of<ILogger<AutomationVersion>>();

            //context accessor
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(It.IsAny<string>());

            //instance of necessary repositories
            var jobRepository = new JobRepository(context, jobLogger, httpContextAccessor.Object);
            var agentRepo = new AgentRepository(context, agentLogger, httpContextAccessor.Object);
            var automationRepo = new AutomationRepository(context, processLogger, httpContextAccessor.Object);
            var jobParameterRepo = new JobParameterRepository(context, jobParameterLogger, httpContextAccessor.Object);
            var jobCheckpointRepo = new JobCheckpointRepository(context, jobCheckpointLogger, httpContextAccessor.Object);
            var automationVersionRepo = new AutomationVersionRepository(context, automationVersionLogger, httpContextAccessor.Object);

            //manager to be tested
            _manager = new JobManager(jobRepository, agentRepo, automationRepo, jobParameterRepo, jobCheckpointRepo, automationVersionRepo);
        }

        //gets the next job that has not been picked up for the specified agent id
        [Fact]
        public async Task GetNextJob()
        {
            //act
            var jobsAvailable = _manager.GetNextJob(_newJobAgentId);
            var jobsCompleted  = _manager.GetNextJob(_completedJobAgentId);

            //assert
            Assert.True(jobsAvailable.IsJobAvailable); //agent id with a new job
            Assert.False(jobsCompleted.IsJobAvailable); //agent id without a new job
        }

        //get the job parameters for the specified job id
        [Fact]
        public async Task GetJobParameters()
        {
            //act
            var jobParameters = _manager.GetJobParameters(_newJobId);

            //assert
            Assert.Equal(_jobParameter,jobParameters.First());
        }

        //delete job parameters for the specified job id
        [Fact]
        public async Task DeleteExistingParameters()
        {
            //act
            _manager.DeleteExistingParameters(_newJobId);
            var jobParameters = _manager.GetJobParameters(_newJobId);

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
