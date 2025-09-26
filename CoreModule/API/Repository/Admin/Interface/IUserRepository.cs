using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Admin;
using Entities.Core;

namespace Repository.Admin.Interface
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
        Task<User> AddAsync(User user);
        Task<User> UpdateAsync(User user);
        Task DeleteAsync(Guid id);
        Task<User?> GetByUserHandleAsync(string userHandle);
        Task<User?> GetByEmailAsync(string email);
      
    }
}
