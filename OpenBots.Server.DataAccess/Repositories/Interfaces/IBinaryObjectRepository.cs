using Microsoft.AspNetCore.Http;
using OpenBots.Server.Model;
using System.Threading.Tasks;

namespace OpenBots.Server.DataAccess.Repositories
{
    /// <summary>
    /// Binary Object Repository Interface
    /// </summary>
    public interface IBinaryObjectRepository : IEntityRepository<BinaryObject>
    {
        public Task UpdateEntity(IFormFile file, string filePath, string binaryObjectId, string apiComponent, string folder, string name, string hash);
    }
}
