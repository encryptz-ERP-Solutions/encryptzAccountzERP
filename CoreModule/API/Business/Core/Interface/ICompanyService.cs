using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;
using Entities.Core;

namespace BusinessLogic.Core.Interface
{
    public interface ICompanyService
    {
        Task<IEnumerable<CompanyDto?>> GetAllCompanyAsync();
        Task<CompanyDto?> GetCompanyByIdAsync(long id);
        Task<bool> AddCompanyAsync(CompanyDto Company);
        Task<bool> UpdateCompanyAsync(CompanyDto Company);
        Task<bool> DeleteCompanyAsync(long id);
    }
}
