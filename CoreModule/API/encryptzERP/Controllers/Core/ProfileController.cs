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
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ExceptionHandler _exceptionHandler;

        public ProfileController(IUserProfileService userProfileService, ExceptionHandler exceptionHandler)
        {
            _userProfileService = userProfileService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = AuthHelper.GetCurrentUserId(HttpContext);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var profile = await _userProfileService.GetProfileAsync(userId.Value);
                if (profile == null)
                {
                    return NotFound();
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "An error occurred while retrieving the profile." });
            }
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                var userId = AuthHelper.GetCurrentUserId(HttpContext);
                if (userId == null)
                {
                    return Unauthorized();
                }

                var updated = await _userProfileService.UpdateProfileAsync(userId.Value, updateDto);
                if (!updated)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(new { message = "Profile updated successfully.", isProfileComplete = true });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "An error occurred while updating the profile." });
            }
        }
    }
}

