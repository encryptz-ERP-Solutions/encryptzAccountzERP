using Entities.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.Core.Interface
{
    public interface IBusinessRepository
    {
        Task<IEnumerable<Business>> GetAllAsync();
        Task<Business> GetByIdAsync(long id);
        Task<Business> AddAsync(Business business);
        Task<bool> UpdateAsync(Business business);
        Task<bool> DeleteAsync(long id);
    }
}