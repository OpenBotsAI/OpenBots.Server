using Microsoft.AspNetCore.Http;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.Model;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public class ProcessManager : BaseManager, IProcessManager
    {
        private readonly IProcessRepository repo;
        private readonly IBinaryObjectRepository binaryObjectRepository;
        private readonly IBinaryObjectManager binaryObjectManager;
        private readonly IBlobStorageAdapter blobStorageAdapter;

        public ProcessManager(IProcessRepository repo,
            IBinaryObjectManager binaryObjectManager,
            IBinaryObjectRepository binaryObjectRepository,
            IBlobStorageAdapter blobStorageAdapter)
        {
            this.repo = repo;
            this.binaryObjectManager = binaryObjectManager;
            this.binaryObjectRepository = binaryObjectRepository;
            this.blobStorageAdapter = blobStorageAdapter;
        }

        public async Task<FileObjectViewModel> Export(string binaryObjectId)
        {
            return await blobStorageAdapter.FetchFile(binaryObjectId);
        }

        public bool DeleteProcess(Guid processId)
        {
            var process = repo.GetOne(processId);
            bool isDeleted = false;

            if (process != null)
            {
                binaryObjectRepository.SoftDelete(process.BinaryObjectId);
                repo.SoftDelete(process.Id.Value);

                isDeleted = true;
            }

            return isDeleted;
        }

        public Process AssignProcessProperties(Process request, Guid versionId)
        {
            int processVersion = 0;

            List<Process> processes = repo.Find(null, x => x.Name.Trim().ToLower() == request.Name.Trim().ToLower())?.Items;

            if (processes != null)
                foreach (Process process in processes)
                {
                    if (processVersion < process.Version)
                    {
                        processVersion = process.Version;
                        versionId = process.VersionId;
                    }
                }

            request.Version = processVersion + 1;
            if (string.IsNullOrEmpty(request.Status))
                request.Status = "Published";
            request.VersionId = versionId;

            return request;
        }

        public Process UpdateProcess(Process requestObj, ProcessViewModel request, Guid versionId)
        {
            int processVersion = requestObj.Version;

            List<Process> processes = repo.Find(null, x => x.VersionId == requestObj.VersionId)?.Items;

            if (processes != null)
                foreach (Process process in processes)
                {
                    if (processVersion < process.Version)
                    {
                        processVersion = process.Version;
                        versionId = process.VersionId;
                    }
                }

                requestObj.Version = processVersion + 1;
                requestObj.VersionId = versionId;
                requestObj.Name = request.Name;
                requestObj.Id = Guid.NewGuid();
                requestObj.Status = "Published";

                return repo.Add(requestObj);
        }

        public async Task<string> Update(Guid binaryObjectId, IFormFile file, string organizationId = "", string apiComponent = "", string name = "")
        {
            //Update file in OpenBots.Server.Web using relative directory
            binaryObjectManager.Update(file, organizationId, apiComponent, binaryObjectId);

            //find relative directory where binary object is being saved
            string filePath = Path.Combine("BinaryObjects", organizationId, apiComponent, binaryObjectId.ToString());

            await binaryObjectManager.UpdateEntity(file, filePath, binaryObjectId.ToString(), apiComponent, apiComponent, name);

            return "Success";
        }

        public string GetOrganizationId()
        {
            string organizationId = binaryObjectManager.GetOrganizationId();

            return organizationId;
        }
    }
}