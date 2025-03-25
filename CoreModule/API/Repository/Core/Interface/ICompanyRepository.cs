using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Entities.Core;

namespace Repository.Core.Interface
{
    public interface ICompanyRepository
    {
        Task<IEnumerable<Company>> GetAllAsync();
        Task<Company> GetByIdAsync(long id);
        Task AddAsync(Company Company);
        Task UpdateAsync(Company Company);
        Task DeleteAsync(long id);
    }
}
