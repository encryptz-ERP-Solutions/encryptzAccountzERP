using BusinessLogic.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Interface
{
    public interface IBusinessService
    {
        Task<IEnumerable<BusinessDto>> GetAllBusinessesAsync();
        Task<BusinessDto> GetBusinessByIdAsync(long id);
        Task<BusinessDto> AddBusinessAsync(BusinessDto businessDto);
        Task<bool> UpdateBusinessAsync(long id, BusinessDto businessDto);
        Task<bool> DeleteBusinessAsync(long id);
    }
}