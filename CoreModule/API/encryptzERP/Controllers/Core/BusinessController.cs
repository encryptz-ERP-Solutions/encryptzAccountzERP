using System;
using System.Collections.Generic;
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
    [Authorize]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public BusinessController(IBusinessService businessService)
        {
            _businessService = businessService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BusinessDto>>> GetAll()
        {
            var businesses = await _businessService.GetAllBusinessesAsync();
            return Ok(businesses);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BusinessDto>> GetById(Guid id)
        {
            var business = await _businessService.GetBusinessByIdAsync(id);
            if (business == null)
            {
                return NotFound();
            }
            return Ok(business);
        }

        [HttpPost]
        [Authorize(Policy = "CanManageBusinesses")]
        public async Task<IActionResult> Create([FromBody] BusinessCreateDto businessCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var newBusiness = await _businessService.CreateBusinessAsync(businessCreateDto, userId);
            return CreatedAtAction(nameof(GetById), new { id = newBusiness.BusinessID }, newBusiness);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] BusinessUpdateDto businessUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var success = await _businessService.UpdateBusinessAsync(id, businessUpdateDto, userId);
            if (!success)
            {
                return NotFound(new { message = "Business not found or update failed." });
            }

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _businessService.DeleteBusinessAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Business not found or could not be deleted." });
            }
            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdString, out var userId);
            return userId;
        }
    }
}