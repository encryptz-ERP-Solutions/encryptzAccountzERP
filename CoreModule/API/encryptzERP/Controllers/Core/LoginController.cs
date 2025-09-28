using BusinessLogic.Admin.DTOs;
using BusinessLogic.Admin.Interface;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                var result = await _loginService.LoginAsync(loginRequest);
                if (result.Token == null || result.Token == "")
                    return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
        }

        [HttpDelete]
        public async Task<ActionResult> Logout(string userId)
        {
            try
            {
                var result = await _loginService.LogoutAsync(userId);
                if (!result)
                    return NotFound();
                return Ok($"{userId} logged out sccessfully..!");
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                LoginResponse loginResponse = new LoginResponse();

                loginResponse = await _loginService.RefreshTokenAsync(request);
                if (loginResponse.Token != null || loginResponse.Token == "")
                    return Unauthorized("Invalid or expired refresh token");

                return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }

        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOTP([FromBody] SendOtpRequest request)
        {
            try
            {
                var response= await _loginService.SendOTP(request);
                if (!response.Item1)
                {
                    return BadRequest(new { status = true, Message = "OTP sent successfully" });
                }
                return Ok(new { status = true, Message = "OTP sent successfully" });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOtpRequest request)
        {
            try
            {
                LoginResponse loginResponse = new LoginResponse();
                var response = await _loginService.VerifyOTP(request);
                if (response == null || response.Token == null || response.Token == "")
                {
                    return BadRequest(new { status = false, Message = "OTP verification failed" });
                }
                return Ok(new { status = true, response = response, Message = "OTP verified successfully" });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
        }


    }
}
