using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Core;

namespace Repository.Core.Interface
{
    public interface IPermissionRepository
    {
        Task<IEnumerable<Permission>> GetAllAsync();
    }
}