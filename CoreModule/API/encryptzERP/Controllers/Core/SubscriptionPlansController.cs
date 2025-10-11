using Business.Core;
using Business.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace encryptzERP.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionPlansController : ControllerBase
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService;

        public SubscriptionPlansController(ISubscriptionPlanService subscriptionPlanService)
        {
            _subscriptionPlanService = subscriptionPlanService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetAll()
        {
            var subscriptionPlans = await _subscriptionPlanService.GetAllAsync();
            return Ok(subscriptionPlans);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionPlanDto>> GetById(int id)
        {
            var subscriptionPlan = await _subscriptionPlanService.GetByIdAsync(id);
            if (subscriptionPlan == null)
            {
                return NotFound();
            }
            return Ok(subscriptionPlan);
        }

        [HttpPost]
        public async Task<ActionResult<SubscriptionPlanDto>> Create(CreateSubscriptionPlanDto createSubscriptionPlanDto)
        {
            var createdSubscriptionPlan = await _subscriptionPlanService.CreateAsync(createSubscriptionPlanDto);
            return CreatedAtAction(nameof(GetById), new { id = createdSubscriptionPlan.PlanID }, createdSubscriptionPlan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateSubscriptionPlanDto updateSubscriptionPlanDto)
        {
            var updatedSubscriptionPlan = await _subscriptionPlanService.UpdateAsync(id, updateSubscriptionPlanDto);
            if (updatedSubscriptionPlan == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _subscriptionPlanService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
