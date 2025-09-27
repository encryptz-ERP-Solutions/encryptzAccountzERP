using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;

namespace BusinessLogic.Core.Interface
{
    public interface IBusinessService
    {
        Task<IEnumerable<BusinessDto>> GetAllBusinessesAsync();
        Task<BusinessDto?> GetBusinessByIdAsync(Guid id);
        Task<BusinessDto> CreateBusinessAsync(BusinessCreateDto businessCreateDto, Guid creatingUserId);
        Task<bool> UpdateBusinessAsync(Guid id, BusinessUpdateDto businessUpdateDto, Guid updatingUserId);
        Task<bool> DeleteBusinessAsync(Guid id);
    }
}