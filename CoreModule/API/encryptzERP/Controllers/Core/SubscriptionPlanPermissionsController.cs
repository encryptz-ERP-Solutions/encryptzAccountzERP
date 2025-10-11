using Business.Core.DTOs;
using Business.Core;
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
    public class SubscriptionPlanPermissionsController : ControllerBase
    {
        private readonly ISubscriptionPlanPermissionService _subscriptionPlanPermissionService;
        private readonly ExceptionHandler _exceptionHandler;

        public SubscriptionPlanPermissionsController(ISubscriptionPlanPermissionService subscriptionPlanPermissionService, ExceptionHandler exceptionHandler)
        {
            _subscriptionPlanPermissionService = subscriptionPlanPermissionService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanPermissionDto>>> GetAll()
        {
            try
            {
                var result = await _subscriptionPlanPermissionService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpGet("{planId}/{permissionId}")]
        public async Task<ActionResult<SubscriptionPlanPermissionDto>> GetById(int planId, int permissionId)
        {
            try
            {
                var result = await _subscriptionPlanPermissionService.GetByIdAsync(planId, permissionId);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpGet("plan/{planId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanPermissionDto>>> GetByPlanId(int planId)
        {
            try
            {
                var result = await _subscriptionPlanPermissionService.GetByPlanIdAsync(planId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpGet("permission/{permissionId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanPermissionDto>>> GetByPermissionId(int permissionId)
        {
            try
            {
                var result = await _subscriptionPlanPermissionService.GetByPermissionIdAsync(permissionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSubscriptionPlanPermissionDto createSubscriptionPlanPermissionDto)
        {
            try
            {
                var response = await _subscriptionPlanPermissionService.CreateAsync(createSubscriptionPlanPermissionDto);
                if (response == null)
                    return BadRequest("Failed to add subscription plan permission.");

                return Ok(new { message = "Subscription plan permission added successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPut("{planId}/{permissionId}")]
        public async Task<IActionResult> Update(int planId, int permissionId, UpdateSubscriptionPlanPermissionDto updateSubscriptionPlanPermissionDto)
        {
            try
            {
                var response = await _subscriptionPlanPermissionService.UpdateAsync(planId, permissionId, updateSubscriptionPlanPermissionDto);
                if (response == null)
                    return NotFound();

                return Ok(new { message = "Subscription plan permission updated successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpDelete("{planId}/{permissionId}")]
        public async Task<IActionResult> Delete(int planId, int permissionId)
        {
            try
            {
                var result = await _subscriptionPlanPermissionService.DeleteAsync(planId, permissionId);
                if (!result)
                    return NotFound();

                return Ok(new { message = "Subscription plan permission deleted successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
