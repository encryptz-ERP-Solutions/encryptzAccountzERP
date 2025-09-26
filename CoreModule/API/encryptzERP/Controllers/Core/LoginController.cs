using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace encryptzERP.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _loginService.LoginAsync(loginRequest);

            if (!response.IsSuccess)
            {
                return Unauthorized(new { response.Message });
            }

            return Ok(response);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdString, out var userId))
            {
                return Unauthorized();
            }

            var result = await _loginService.ChangePasswordAsync(userId, changePasswordDto);

            if (!result)
            {
                return BadRequest(new { Message = "Failed to change password. Please check your old password." });
            }

            return Ok(new { Message = "Password changed successfully." });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _loginService.ForgotPasswordAsync(forgotPasswordDto);

            // Always return OK to prevent user enumeration
            return Ok(new { Message = "If an account with that identifier exists, a password reset OTP has been sent." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _loginService.ResetPasswordAsync(resetPasswordDto);

            if (!result)
            {
                return BadRequest(new { Message = "Password reset failed. The OTP may be invalid or expired." });
            }

            return Ok(new { Message = "Password has been reset successfully." });
        }

        [HttpPost("request-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestLoginOtp([FromBody] OtpLoginRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _loginService.RequestLoginOtpAsync(request);

            // Always return OK to prevent user enumeration
            return Ok(new { Message = "If an account with that identifier is registered, an OTP has been sent." });
        }

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyLoginOtp([FromBody] OtpLoginVerifyDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _loginService.VerifyLoginOtpAsync(request);

            if (!response.IsSuccess)
            {
                return Unauthorized(new { response.Message });
            }

            return Ok(response);
        }
    }
}