using OpenBots.Server.Model.Core;
using OpenBots.Server.ViewModel.File;
using System;
using System.Threading.Tasks;

namespace OpenBots.Server.Business.Interfaces
{
    public interface IFileStorageAdapter
    {
        public Task<FileFolderViewModel> ExportFile(string id);

        public PaginatedList<FileFolderViewModel> GetFilesFolders(bool? isFile = null, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);

        public FileFolderViewModel SaveFile(FileFolderViewModel viewModel);

        public void UpdateFile(UpdateServerFileViewModel request);

        public void DeleteFile(string path);
    }
}
