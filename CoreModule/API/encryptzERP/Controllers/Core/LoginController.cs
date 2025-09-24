using BusinessLogic.Admin.DTOs;
using BusinessLogic.Admin.Interface;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Microsoft.AspNetCore.Http;
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

        [HttpPost]
        public async Task<ActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var result = await _loginService.LoginAsync(loginRequest);
            if (result.Token == null || result.Token == "")
                return NotFound();
            return Ok(result);
        }

        [HttpDelete]
        public async Task<ActionResult> Logout(string userId)
        {
            var result = await _loginService.LogoutAsync(userId);
            if (!result)
                return NotFound();
            return Ok($"{userId} logged out sccessfully..!");
        }

        [HttpPost("refresh")]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            LoginResponse loginResponse = new LoginResponse();

            loginResponse = await _loginService.RefreshTokenAsync(request);
            if (loginResponse.Token != null || loginResponse.Token == "")
                return Unauthorized("Invalid or expired refresh token");

            return Ok(loginResponse);
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOTP([FromBody] SendOtpRequest request)
        {
            var response = await _loginService.SendOTP(request);
            if (!response.Item1)
            {
                return BadRequest(new { status = true, Message = "OTP sent successfully" });
            }
            return Ok(new { status = true, Message = "OTP sent successfully" });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOtpRequest request)
        {
            LoginResponse loginResponse = new LoginResponse();
            var response = await _loginService.VerifyOTP(request);
            if (response == null || response.Token == null || response.Token == "")
            {
                return BadRequest(new { status = false, Message = "OTP verification failed" });
            }
            return Ok(new { status = true, response = response, Message = "OTP verified successfully" });
        }


    }
}
