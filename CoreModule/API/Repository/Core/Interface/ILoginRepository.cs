using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Admin;



namespace Repository.Core.Interface
{
    public interface ILoginRepository
    {
        Task<User> LoginAsync(string userName,string password);
        Task<bool> SaveOTP(string loginType, string loginId, string otp, string fullName);
        Task<bool> VerifyOTP(string loginType, string loginId, string otp);
        Task<bool> ChangePassword(int userId, string newPassword);
        Task<int?> GetUserIdByEmail(string email);
        Task<int?> GetMaxofUserId();
    }
}
