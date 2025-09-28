using System;
using System.Threading.Tasks;

namespace Repository.Core.Interface
{
    public interface ILoginRepository
    {
        /// <summary>
        /// Saves a one-time password (OTP) for a given login identifier (e.g., email).
        /// </summary>
        Task<bool> SaveOTPAsync(string loginIdentifier, string otp);

        /// <summary>
        /// Verifies if the provided OTP is valid for the given login identifier.
        /// </summary>
        Task<bool> VerifyOTPAsync(string loginIdentifier, string otp);

        /// <summary>
        /// Updates the password for a specific user.
        /// </summary>
        Task<bool> ChangePasswordAsync(Guid userId, string newHashedPassword);
    }
}