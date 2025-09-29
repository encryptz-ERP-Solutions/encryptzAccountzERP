using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace encryptzERP.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;
        private readonly ExceptionHandler _exceptionHandler;

        public LoginController(ILoginService loginService, ExceptionHandler exceptionHandler)
        {
            _loginService = loginService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto loginRequestDto)
        {
            try
            {
                var response = await _loginService.LoginAsync(loginRequestDto);
                if (!response.IsSuccess)
                {
                    return Unauthorized(new { response.Message });
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred during login.");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto forgotPasswordDto)
        {
            try
            {
                await _loginService.ForgotPasswordAsync(forgotPasswordDto);
                return Ok(new { message = "If a user with that email exists, an OTP has been sent." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var success = await _loginService.ResetPasswordAsync(resetPasswordDto);
                if (!success)
                {
                    return BadRequest(new { message = "Invalid OTP or failed to reset password." });
                }
                return Ok(new { message = "Password has been reset successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized();
                }

                var success = await _loginService.ChangePasswordAsync(userId, changePasswordDto);
                if (!success)
                {
                    return BadRequest(new { message = "Failed to change password. Please check your old password." });
                }
                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp(OtpRequestDto otpRequestDto)
        {
            try
            {
                await _loginService.RequestOtpAsync(otpRequestDto);
                return Ok(new { message = "If an account with that identifier exists, an OTP has been sent." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPost("verify-otp")]
        public async Task<ActionResult<LoginResponseDto>> VerifyOtp(OtpVerifyDto otpVerifyDto)
        {
            try
            {
                var response = await _loginService.VerifyOtpAsync(otpVerifyDto);
                if (!response.IsSuccess)
                {
                    return BadRequest(new { message = response.Message });
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}