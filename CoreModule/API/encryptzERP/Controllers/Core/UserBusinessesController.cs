using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace encryptzERP.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserBusinessesController : ControllerBase
    {
        private readonly IUserBusinessService _userBusinessService;
        private readonly IAuditService _auditService;
        private readonly ExceptionHandler _exceptionHandler;

        public UserBusinessesController(
            IUserBusinessService userBusinessService,
            IAuditService auditService,
            ExceptionHandler exceptionHandler)
        {
            _userBusinessService = userBusinessService;
            _auditService = auditService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserBusinessDto>>> GetByUserId([FromQuery] Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest("userId is required");
                }

                var result = await _userBusinessService.GetByUserIdAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserBusinessRequest request)
        {
            try
            {
                if (request.UserID == Guid.Empty || request.BusinessID == Guid.Empty)
                {
                    return BadRequest("UserID and BusinessID are required");
                }

                var currentUserId = AuthHelper.GetCurrentUserId(HttpContext);
                var result = await _userBusinessService.CreateAsync(request, currentUserId);

                // Audit log
                await _auditService.LogAsync(
                    currentUserId,
                    "INSERT",
                    "user_businesses",
                    result.UserBusinessID.ToString(),
                    $"Linked user {request.UserID} to business {request.BusinessID}");

                return CreatedAtAction(nameof(GetByUserId), new { userId = request.UserID }, result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPost("{id}/set-default")]
        public async Task<IActionResult> SetDefault(Guid id, [FromQuery] Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest("userId is required");
                }

                var currentUserId = AuthHelper.GetCurrentUserId(HttpContext);
                var success = await _userBusinessService.SetDefaultAsync(id, userId, currentUserId);

                if (!success)
                {
                    return NotFound(new { message = "User business link not found or does not belong to the specified user." });
                }

                // Audit log
                await _auditService.LogAsync(
                    currentUserId,
                    "UPDATE",
                    "user_businesses",
                    id.ToString(),
                    $"Set as default business for user {userId}");

                return Ok(new { message = "Default business set successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var currentUserId = AuthHelper.GetCurrentUserId(HttpContext);
                var success = await _userBusinessService.DeleteAsync(id);

                if (!success)
                {
                    return NotFound(new { message = "User business link not found." });
                }

                // Audit log
                await _auditService.LogAsync(
                    currentUserId,
                    "DELETE",
                    "user_businesses",
                    id.ToString(),
                    "Removed user-business link");

                return Ok(new { message = "User business link deleted successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}

