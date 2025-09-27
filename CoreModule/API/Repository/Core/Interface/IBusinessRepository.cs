using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Core;

namespace Repository.Core.Interface
{
    public interface IBusinessRepository
    {
        Task<IEnumerable<Business>> GetAllAsync();
        Task<Business?> GetByIdAsync(Guid id);
        Task<Business> AddAsync(Business business);
        Task<Business> UpdateAsync(Business business);
        Task<bool> DeleteAsync(Guid id);
    }
}