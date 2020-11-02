using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Security;
using OpenBots.Server.ViewModel;
using System;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTests
{
    public class AgentManagerTests
    {
        [Fact]
        public async Task TestAgentManager()
        {
            // arrange
            var options = new DbContextOptionsBuilder<StorageContext>()
                .UseInMemoryDatabase(databaseName: "AgentManager")
                .Options;

            Guid credentialId = Guid.NewGuid();
            Guid agentId = Guid.NewGuid();

            var context = new StorageContext(options);
            var dummyAgent = new AgentModel
            {
                Id = agentId,
                Name = "TesterAgent",
                MachineName = "TestingMachine",
                MacAddresses = "00:00:00:a1:2b:cc",
                IPAddresses = "192.165.1.91",
                CredentialId = credentialId                
            };

            var dummyCredential = new Credential
            {
                Id = credentialId,
                Name = "TesterCredential"
            };

            var dummyUserAgent = new AspNetUsers
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "TesterUser",
                Name = "TesterAgent"
            };
            var dummyJob = new Job
            {
                Id = Guid.NewGuid(),
                AgentId = agentId,
                JobStatus = JobStatusType.New
            };

            Seed(context, dummyAgent, dummyCredential, dummyUserAgent, dummyJob);

            var agentLogger = Mock.Of<ILogger<AgentModel>>();
            var usersLogger = Mock.Of<ILogger<AspNetUsers>>();
            var scheduleLogger = Mock.Of<ILogger<Schedule>>();
            var jobLogger = Mock.Of<ILogger<Job>>();
            var credentialLogger = Mock.Of<ILogger<Credential>>();

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(It.IsAny<string>());

            var userRepo = new AspNetUsersRepository(context, usersLogger, httpContextAccessor.Object);
            var agentRepo = new AgentRepository(context, agentLogger, httpContextAccessor.Object);
            var scheduleRepo = new ScheduleRepository(context, scheduleLogger, httpContextAccessor.Object);
            var jobRepo = new JobRepository(context, jobLogger, httpContextAccessor.Object);
            var credentialRepo = new CredentialRepository(context, credentialLogger, httpContextAccessor.Object);
            var manager = new AgentManager(agentRepo, scheduleRepo, jobRepo,userRepo, credentialRepo);

            //act
            AgentViewModel view = new AgentViewModel();
            AgentModel agentModel = agentRepo.GetOne(agentId);
            view = view.Map(agentModel);

            var validAgentView = manager.GetAgentDetails(view);//Fetches agent details
            bool agentWithDependant = manager.CheckReferentialIntegrity(agentId.ToString());
             
            dummyJob.JobStatus = JobStatusType.Completed;//Removes referential integrity violation
            bool agentWithoutDependant = manager.CheckReferentialIntegrity(agentId.ToString());
            
            // assert
            Assert.Equal(dummyCredential.Name, validAgentView.CredentialName);
            Assert.Equal(dummyUserAgent.UserName, validAgentView.UserName);
            Assert.True(agentWithDependant);
            Assert.False(agentWithoutDependant);
        }

        private void Seed(StorageContext context, AgentModel agent, Credential credential, AspNetUsers aspNetUser, Job job)
        {
            var agents = new[]
            {
                agent
            };

            var credentials = new[]
            {
                credential
            };

            var aspNetUsers = new[]
            {
                aspNetUser
            };

            var jobs = new[]
            {
                job
            };

            context.Agents.AddRange(agents);
            context.Credentials.AddRange(credentials);
            context.AspNetUsers.AddRange(aspNetUser);
            context.Jobs.Add(job);
            context.SaveChanges();
        }
    }
}
