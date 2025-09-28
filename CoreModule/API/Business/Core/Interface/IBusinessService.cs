using BusinessLogic.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Interface
{
    public interface IBusinessService
    {
        Task<IEnumerable<BusinessDto>> GetAllBusinessesAsync();
        Task<BusinessDto> GetBusinessByIdAsync(Guid id);
        Task<BusinessDto> AddBusinessAsync(BusinessDto businessDto);
        Task<bool> UpdateBusinessAsync(Guid id, BusinessDto businessDto);
        Task<bool> DeleteBusinessAsync(Guid id);
    }
}