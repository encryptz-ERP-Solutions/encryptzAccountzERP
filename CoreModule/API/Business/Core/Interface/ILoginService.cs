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
        /// <param name="loginRequest">The login request data.</param>
        /// <returns>A DTO containing the login result, including a JWT on success.</returns>
        Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);

        /// <summary>
        /// Allows an authenticated user to change their own password.
        /// </summary>
        /// <param name="userId">The ID of the currently authenticated user.</param>
        /// <param name="changePasswordDto">The password change data.</param>
        /// <returns>True if the password was successfully changed, otherwise false.</returns>
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);

        /// <summary>
        /// Initiates the password reset process for a user by generating and sending an OTP.
        /// </summary>
        /// <param name="forgotPasswordRequestDto">The forgot password request data.</param>
        /// <returns>True if the process was initiated successfully, otherwise false.</returns>
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto forgotPasswordRequestDto);

        /// <summary>
        /// Resets a user's password using a valid OTP.
        /// </summary>
        /// <param name="resetPasswordDto">The password reset data.</param>
        /// <returns>True if the password was successfully reset, otherwise false.</returns>
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        /// <summary>
        /// Initiates a passwordless login by sending an OTP to the user.
        /// </summary>
        /// <param name="request">The OTP login request data.</param>
        /// <returns>True if the process was initiated successfully, otherwise false.</returns>
        Task<bool> RequestLoginOtpAsync(OtpLoginRequestDto request);

        /// <summary>
        /// Verifies an OTP to complete a passwordless login.
        /// </summary>
        /// <param name="request">The OTP verification data.</param>
        /// <returns>A DTO containing the login result, including a JWT on success.</returns>
        Task<LoginResponseDto> VerifyLoginOtpAsync(OtpLoginVerifyDto request);
    }
}