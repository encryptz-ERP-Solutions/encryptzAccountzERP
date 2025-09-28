using System;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;

namespace BusinessLogic.Core.Interface
{
    /// <summary>
    /// Handles user authentication and password management logic.
    /// </summary>
    public interface ILoginService
    {
        /// <summary>
        /// Authenticates a user based on their login identifier and password.
        /// </summary>
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);

        /// <summary>
        /// Allows an authenticated user to change their own password.
        /// </summary>
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);

        /// <summary>
        /// Initiates the password reset process for a user by generating and sending an OTP.
        /// </summary>
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequestDto);

        /// <summary>
        /// Resets a user's password using a valid OTP.
        /// </summary>
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        /// <summary>
        /// Initiates sending an OTP to a user.
        /// </summary>
        Task<bool> RequestOtpAsync(OtpRequestDto otpRequestDto);

        /// <summary>
        /// Verifies an OTP provided by a user.
        /// </summary>
        Task<bool> VerifyOtpAsync(OtpVerifyDto otpVerifyDto);
    }
}