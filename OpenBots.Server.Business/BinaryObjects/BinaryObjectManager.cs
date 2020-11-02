using Microsoft.AspNetCore.Http;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.ViewModel;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using OpenBots.Server.Model;
using System.IO;
using System.Text;

namespace OpenBots.Server.Business
{
    public class BinaryObjectManager : BaseManager, IBinaryObjectManager
    {
        private readonly IBlobStorageAdapter blobStorageAdapter;
        private readonly IFileSystemAdapter fileSystemAdapter;
        private readonly IOrganizationMemberRepository organizationMemberRepository;
        private readonly IOrganizationManager organizationManager;
        private readonly ClaimsPrincipal _caller;
        private readonly IAspNetUsersRepository usersRepository;
        public IConfiguration Configuration { get; }

        public BinaryObjectManager(
            IBlobStorageAdapter blobStorageAdapter,
            IFileSystemAdapter fileSystemAdapter,
            IHttpContextAccessor httpContextAccessor,
            IPersonEmailRepository personEmailRepository,
            IOrganizationMemberRepository organizationMemberRepository,
            IOrganizationManager organizationManager,
            IConfiguration configuration,
            IAspNetUsersRepository usersRepository)
        {
            this.blobStorageAdapter = blobStorageAdapter;
            this.fileSystemAdapter = fileSystemAdapter;
            this.organizationMemberRepository = organizationMemberRepository;
            this.organizationManager = organizationManager;
            this.usersRepository = usersRepository;
            Configuration = configuration;
            _caller = ((httpContextAccessor.HttpContext != null) ? httpContextAccessor.HttpContext.User : new ClaimsPrincipal());
        }

        public string Upload(IFormFile file, string organizationId, string apiComponent, string binaryObjectId)
        {
            string adapter = Configuration["BinaryObjects:Adapter"];
            if (adapter.Equals("FileSystemAdapter"))
            {
                string path = Configuration["BinaryObjects:Path"];
                string binaryObjId = fileSystemAdapter.SaveFile(file, path, organizationId, apiComponent, binaryObjectId);
                return binaryObjId;
            }
            return "Configuration for Azure Blob Storage is not set up yet.";
        }

        public void SaveEntity(IFormFile file, string filePath, BinaryObject binaryObject, string apiComponent, string organizationId)
        {
            string storageProvider = string.Empty;
            string adapter = Configuration["BinaryObjects:Adapter"];
            if (adapter.Equals("FileSystemAdapter"))
                storageProvider = Configuration["BinaryObjects:StorageProvider"];

            string hash = string.Empty;
            byte[] bytes = File.ReadAllBytes(filePath);
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = GetHash(sha256Hash, bytes);
            }

            blobStorageAdapter.SaveEntity(file, filePath, binaryObject, apiComponent, organizationId, storageProvider, hash);
        }

        private static string GetHash(HashAlgorithm hashAlgorithm, byte[] input)
        {
            //Convert the input string to a byte array and compute the hash
            byte[] data = hashAlgorithm.ComputeHash(input);
            //Create new StringBuilder to collect the bytes and create a string
            var sBuilder = new StringBuilder();
            //Loop through each byte of the hashed data and format each one as a hexidecimal string
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));
            //Return the hexidecimal string
            return sBuilder.ToString();
        }

        public async Task<FileObjectViewModel> Download(string binaryObjectId)
        {
            return await blobStorageAdapter.FetchFile(binaryObjectId);
        }

        public void Update(IFormFile file, string organizationId, string apiComponent, Guid binaryObjectId)
        {
            string adapter = Configuration["BinaryObjects:Adapter"];
            if (adapter.Equals("FileSystemAdapter"))
            {
                string path = Configuration["BinaryObjects:Path"];
                fileSystemAdapter.UpdateFile(file, path, organizationId, apiComponent, binaryObjectId);
            }
        }

        public async Task<string> UpdateEntity(IFormFile file, string filePath, string binaryObjectId, string apiComponent, string folder, string name)
        {
            string hash = string.Empty;
            byte[] bytes = File.ReadAllBytes(filePath);
            using (SHA256 sha256Hash = SHA256.Create())
            {
                hash = GetHash(sha256Hash, bytes);
            }

            await blobStorageAdapter.UpdateEntity(file, filePath, binaryObjectId, apiComponent, folder, name, hash);
            return "Success";
        }

        public string GetOrganizationId()
        {
            string identity = _caller.Identity.Name;
            var user = usersRepository.Find(null, u => u.UserName == identity).Items?.FirstOrDefault();
            organizationMemberRepository.ForceIgnoreSecurity();
            var orgMember = organizationMemberRepository.Find(null, om => om.PersonId.Equals(user.PersonId))?.Items?.FirstOrDefault();
            string organizationId;
            if (orgMember == null)
                organizationId = organizationManager.GetDefaultOrganization().Id.ToString();
            else organizationId = orgMember.OrganizationId.ToString();

            return organizationId;
        }
    }
}