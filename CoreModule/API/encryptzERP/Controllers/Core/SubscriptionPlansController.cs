using Business.Core;
using Business.Core.DTOs;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace encryptzERP.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService;
        private readonly ExceptionHandler _exceptionHandler;

        public SubscriptionPlansController(ISubscriptionPlanService subscriptionPlanService, ExceptionHandler exceptionHandler)
        {
            _subscriptionPlanService = subscriptionPlanService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetAll()
        {
            try
            {
                var subscriptionPlans = await _subscriptionPlanService.GetAllAsync();
                return Ok(subscriptionPlans);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An error occurred while retrieving subscription plans.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionPlanDto>> GetById(int id)
        {
            try
            {
                var subscriptionPlan = await _subscriptionPlanService.GetByIdAsync(id);
                if (subscriptionPlan == null)
                {
                    return NotFound($"Subscription plan with ID {id} not found.");
                }
                return Ok(subscriptionPlan);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An error occurred while retrieving the subscription plan.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<SubscriptionPlanDto>> Create(CreateSubscriptionPlanDto createSubscriptionPlanDto)
        {
            try
            {
                if (createSubscriptionPlanDto == null)
                {
                    return BadRequest("Subscription plan data is required.");
                }

                var createdSubscriptionPlan = await _subscriptionPlanService.CreateAsync(createSubscriptionPlanDto);
                return CreatedAtAction(nameof(GetById), new { id = createdSubscriptionPlan.PlanID }, createdSubscriptionPlan);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An error occurred while creating the subscription plan.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateSubscriptionPlanDto updateSubscriptionPlanDto)
        {
            try
            {
                if (updateSubscriptionPlanDto == null)
                {
                    return BadRequest("Subscription plan data is required.");
                }

                var updatedSubscriptionPlan = await _subscriptionPlanService.UpdateAsync(id, updateSubscriptionPlanDto);
                if (updatedSubscriptionPlan == null)
                {
                    return NotFound($"Subscription plan with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An error occurred while updating the subscription plan.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _subscriptionPlanService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Subscription plan with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An error occurred while deleting the subscription plan.");
            }
        }
    }
}
