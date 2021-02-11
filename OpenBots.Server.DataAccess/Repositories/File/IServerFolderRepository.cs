using OpenBots.Server.Model.Core;
using OpenBots.Server.Model.File;
using OpenBots.Server.ViewModel.File;
using System;

namespace OpenBots.Server.DataAccess.Repositories.Interfaces
{
    public interface IServerFolderRepository : IEntityRepository<ServerFolder>
    {
        public PaginatedList<FileFolderViewModel> FindAllView(Guid? driveId, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
        public PaginatedList<FileFolderViewModel> FindAllFilesFoldersView(Guid? driveId, Predicate<FileFolderViewModel> predicate = null, string sortColumn = "", OrderByDirectionType direction = OrderByDirectionType.Ascending, int skip = 0, int take = 100);
    }
}
