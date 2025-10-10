using Business.Core;
using Business.Core.DTOs;
using Microsoft;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace encryptzERP.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserSubscriptionsController : ControllerBase
    {
        private readonly IUserSubscriptionService _userSubscriptionService;

        public UserSubscriptionsController(IUserSubscriptionService userSubscriptionService)
        {
            _userSubscriptionService = userSubscriptionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserSubscriptionDto>>> GetAll()
        {
            var userSubscriptions = await _userSubscriptionService.GetAllAsync();
            return Ok(userSubscriptions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserSubscriptionDto>> GetById(Guid id)
        {
            var userSubscription = await _userSubscriptionService.GetByIdAsync(id);
            if (userSubscription == null)
            {
                return NotFound();
            }
            return Ok(userSubscription);
        }

        [HttpPost]
        public async Task<ActionResult<UserSubscriptionDto>> Create(CreateUserSubscriptionDto createUserSubscriptionDto)
        {
            var createdUserSubscription = await _userSubscriptionService.CreateAsync(createUserSubscriptionDto);
            return CreatedAtAction(nameof(GetById), new { id = createdUserSubscription.SubscriptionID }, createdUserSubscription);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateUserSubscriptionDto updateUserSubscriptionDto)
        {
            var updatedUserSubscription = await _userSubscriptionService.UpdateAsync(id, updateUserSubscriptionDto);
            if (updatedUserSubscription == null)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userSubscriptionService.DeleteAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
