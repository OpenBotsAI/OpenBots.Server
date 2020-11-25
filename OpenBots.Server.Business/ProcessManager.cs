using Microsoft.AspNetCore.Http;
using OpenBots.Server.DataAccess.Repositories;
using OpenBots.Server.DataAccess.Repositories.Interfaces;
using OpenBots.Server.Model;
using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBots.Server.Business
{
    public class ProcessManager : BaseManager, IProcessManager
    {
        private readonly IProcessRepository repo;
        private readonly IBinaryObjectRepository binaryObjectRepository;
        private readonly IBinaryObjectManager binaryObjectManager;
        private readonly IBlobStorageAdapter blobStorageAdapter;
        private readonly IProcessVersionRepository processVersionRepository;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ProcessManager(IProcessRepository repo,
            IBinaryObjectManager binaryObjectManager,
            IBinaryObjectRepository binaryObjectRepository,
            IBlobStorageAdapter blobStorageAdapter,
            IProcessVersionRepository processVersionRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            this.repo = repo;
            this.binaryObjectManager = binaryObjectManager;
            this.binaryObjectRepository = binaryObjectRepository;
            this.blobStorageAdapter = blobStorageAdapter;
            this.processVersionRepository = processVersionRepository;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<FileObjectViewModel> Export(string binaryObjectId)
        {
            return await blobStorageAdapter.FetchFile(binaryObjectId);
        }

        public bool DeleteProcess(Guid processId)
        {
            var process = repo.GetOne(processId);

            // Remove process version entity associated with process
            var processVersion = processVersionRepository.Find(null, q => q.ProcessId == processId).Items?.FirstOrDefault();
            Guid processVersionId = (Guid)processVersion.Id;
            processVersionRepository.SoftDelete(processVersionId);
            
            bool isDeleted = false;

            if (process != null)
            {
                // Remove binary object entity associated with process
                binaryObjectRepository.SoftDelete(process.BinaryObjectId);
                repo.SoftDelete(process.Id.Value);

                isDeleted = true;
            }

            return isDeleted;
        }

        public Process UpdateProcess(Process existingProcess, ProcessViewModel request)
        {
            Process process = new Process()
            {
                Id = request.Id,
                Name = request.Name,
                CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name,
                CreatedOn = DateTime.UtcNow,
                BinaryObjectId = existingProcess.BinaryObjectId,
                OriginalPackageName = existingProcess.OriginalPackageName
            };

            request.Id = process.Id;
            AddProcessVersion(request);  

            return repo.Add(process);
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
            return binaryObjectManager.GetOrganizationId();
        }

        public void AddProcessVersion(ProcessViewModel processViewModel)
        {
            ProcessVersion processVersion = new ProcessVersion();
            processVersion.CreatedBy = httpContextAccessor.HttpContext.User.Identity.Name;
            processVersion.CreatedOn = DateTime.UtcNow;
            processVersion.ProcessId = (Guid)processViewModel.Id;
            if (string.IsNullOrEmpty(processViewModel.Status))
                processVersion.Status = "Published";
            else processVersion.Status = processViewModel.Status;
            processVersion.VersionNumber = processViewModel.VersionNumber;
            if (processVersion.Status.Equals("Published"))
            {
                processVersion.PublishedBy = httpContextAccessor.HttpContext.User.Identity.Name;
                processVersion.PublishedOnUTC = DateTime.UtcNow;
            }
            else
            {
                processVersion.PublishedBy = null;
                processVersion.PublishedOnUTC = null;
            }

            int processVersionNumber = 0;
            processVersion.VersionNumber = processVersionNumber;
            List<Process> processes = repo.Find(null, x => x.Name?.Trim().ToLower() == processViewModel.Name?.Trim().ToLower())?.Items;

            if (processes != null)
                foreach (Process process in processes)
                {
                    var processVersionEntity = processVersionRepository.Find(null, q => q?.ProcessId == process?.Id).Items?.FirstOrDefault();
                    if (processVersionEntity != null && processVersionNumber < processVersionEntity.VersionNumber)
                    {
                        processVersionNumber = processVersionEntity.VersionNumber;
                    }
                }

            processVersion.VersionNumber = processVersionNumber + 1;

            processVersionRepository.Add(processVersion);
        }

        public PaginatedList<AllProcessesViewModel> GetProcessesAndProcessVersions(Predicate<AllProcessesViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100)
        {
            return repo.FindAllView(predicate, sortColumn, direction, skip, take);
        }

        public ProcessViewModel GetProcessView(ProcessViewModel processView, string id)
        {
            var processVersion = processVersionRepository.Find(null, q => q.ProcessId == Guid.Parse(id))?.Items?.FirstOrDefault();
            if (processVersion != null)
            {
                processView.VersionId = (Guid)processVersion.Id;
                processView.VersionNumber = processVersion.VersionNumber;
                processView.Status = processVersion.Status;
                processView.PublishedBy = processVersion.PublishedBy;
                processView.PublishedOnUTC = processVersion.PublishedOnUTC;
            }

            return processView;
        }
    }
}