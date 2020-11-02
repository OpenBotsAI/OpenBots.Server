using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OpenBots.Server.Business;
using OpenBots.Server.DataAccess;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTests
{
    public class BinaryObjectManagerTests
    {
        [Fact]
        public async Task Upload()
        {
            //arrange
            var options = new DbContextOptionsBuilder<StorageContext>()
                .UseInMemoryDatabase(databaseName: "SavesFile")
                .Options;

            var context = new StorageContext(options);
            var dummyBinaryObject = new BinaryObject
            {
                Id = new Guid("EC84D245-0E7C-4CA7-AFA6-DB177BCBB2EE"),
                Name = "Test Binary Object",
                OrganizationId = new Guid("04a348ec-2968-4406-bf4a-4a5fda73df00"),
                ContentType = "Test Content Type",
                CorrelationEntityId = new Guid("04a348ec-2968-4406-bf4a-4a5fda73df00"),
                CorrelationEntity = "Test Correlation Entity",
                StoragePath = @"BinaryObjects\04a348ec-2968-4406-bf4a-4a5fda73df00\TestAPI\bb9a2279-b9b3-47ea-92d5-4c2766a6c232",
                StorageProvider = "Test Provider",
                SizeInBytes = 28,
                HashCode = 55555.ToString()
            };

            Seed(context, dummyBinaryObject);

            var logger = Mock.Of<ILogger<BinaryObject>>();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
                httpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(It.IsAny<string>());
            var directoryManager = new DirectoryManager();
            var blobStorageAdapter = Mock.Of<IBlobStorageAdapter>();
            var personEmailRepository = Mock.Of<IPersonEmailRepository>();
            var organizationMemberRepository = Mock.Of<IOrganizationMemberRepository>();
            var usersRepository = Mock.Of<IAspNetUsersRepository>();
            var organizationManager = Mock.Of<IOrganizationManager>();
            var fileSystemAdapter = new FileSystemAdapter(directoryManager);

            var myConfiguration = new Dictionary<string, string>
            {
                {"BinaryObjects:Adapter", "FileSystemAdapter"},
                {"BinaryObjects:Path", "BinaryObjects"},
                {"BinaryObjects:StorageProvider", "FileSystem.Default"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            var manager = new BinaryObjectManager(blobStorageAdapter, fileSystemAdapter, httpContextAccessor.Object, personEmailRepository, organizationMemberRepository, organizationManager, configuration, usersRepository);

            var testFile = new Mock<IFormFile>();
                testFile.Setup(f => f.Length).Returns(1111);
                testFile.Setup(f => f.FileName).Returns("testFile");
            var nullFile = new Mock<IFormFile>();
                nullFile.Setup(f => f.Length).Returns(0);
                nullFile.Setup(f => f.FileName).Returns("nullFile");

            var organizationId = "04a348ec-2968-4406-bf4a-4a5fda73df00";
            var apiComponent = "TestAPI";

            //act
            var validFile = manager.Upload(testFile.Object, organizationId, apiComponent, dummyBinaryObject.Id.ToString());
            var invalidFile = manager.Upload(nullFile.Object, organizationId, apiComponent, dummyBinaryObject.Id.ToString());

            //assert
            Assert.IsType<Guid>(Guid.Parse(validFile));
            Assert.Equal("No file exists.", invalidFile);

            File.Delete(Path.Combine("BinaryObjects", organizationId, apiComponent, validFile));
        }

        [Fact]
        public async Task Download()
        {
            //arrange
            var options = new DbContextOptionsBuilder<StorageContext>()
                .UseInMemoryDatabase(databaseName: "SavesFile")
                .Options;

            var context = new StorageContext(options);
            var dummyBinaryObject = new BinaryObject
            {
                Id = new Guid("e7d99328-b5f0-4356-80df-2dd61a82505c"),
                Name = "Test Binary Object",
                OrganizationId = new Guid("04a348ec-2968-4406-bf4a-4a5fda73df00"),
                ContentType = "Test Content Type",
                CorrelationEntityId = new Guid("04a348ec-2968-4406-bf4a-4a5fda73df00"),
                CorrelationEntity = "Test Correlation Entity",
                StoragePath = @"BinaryObjects\04a348ec-2968-4406-bf4a-4a5fda73df00\TestAPI\e7d99328-b5f0-4356-80df-2dd61a82505c",
                StorageProvider = "Test Provider",
                SizeInBytes = 28,
                HashCode = 55555.ToString()
            };

            Seed(context, dummyBinaryObject);

            var logger = Mock.Of<ILogger<BinaryObject>>();
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
                httpContextAccessor.Setup(req => req.HttpContext.User.Identity.Name).Returns(It.IsAny<string>());
            var personEmailRepository = Mock.Of<IPersonEmailRepository>();
            var organizationMemberRepository = Mock.Of<IOrganizationMemberRepository>();
            var usersRepository = Mock.Of<IAspNetUsersRepository>();
            var organizationManager = Mock.Of<IOrganizationManager>();
            var repo = new BinaryObjectRepository(context, logger, httpContextAccessor.Object);

            var blobStorageAdapter = new BlobStorageAdapter(repo);
            var directoryManager = new DirectoryManager();
            var fileSystemAdapter = new FileSystemAdapter(directoryManager);
            var mockOptions = Mock.Of<IOptions<ConfigurationValue>>();
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(req => req.GetSection(ConfigurationValue.Values)).Returns(It.IsAny<IConfigurationSection>());
            var manager = new BinaryObjectManager(blobStorageAdapter, fileSystemAdapter, httpContextAccessor.Object, personEmailRepository, organizationMemberRepository, organizationManager, configuration.Object, usersRepository);

            string validBinaryObjectId = dummyBinaryObject.Id.ToString();
            string invalidBinaryObjectId = "9999bbf9-9327-48f7-a5e3-36cdfe4eb6a6";

            //act
            var validFile = manager.Download(validBinaryObjectId);
            var invalidFile = manager.Download(invalidBinaryObjectId);

            //assert
            var storagePath = validFile.Result.StoragePath;
            Assert.True(validFile.Result.StoragePath.Equals("BinaryObjects\\04a348ec-2968-4406-bf4a-4a5fda73df00\\TestAPI\\e7d99328-b5f0-4356-80df-2dd61a82505c"));
            Assert.Null(invalidFile.Result.StoragePath);

        }

        private void Seed(StorageContext context, BinaryObject model)
        {
            var items = new[]
            {
            model
        };

            context.BinaryObjects.AddRange(items);
            context.SaveChanges();
        }
    }
}