using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace encryptzERP.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "CanManageBusinesses")]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private readonly ExceptionHandler _exceptionHandler;

        public BusinessController(IBusinessService businessService, ExceptionHandler exceptionHandler)
        {
            _businessService = businessService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BusinessDto>>> GetAll()
        {
            try
            {
                var result = await _businessService.GetAllBusinessesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BusinessDto>> GetById(Guid id)
        {
            try
            {
                var result = await _businessService.GetBusinessByIdAsync(id);
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

        [HttpPost]
        public async Task<IActionResult> Create(BusinessDto businessDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized("User ID claim is missing or invalid.");
                }

                var newBusiness = await _businessService.AddBusinessAsync(businessDto, userId);
                return CreatedAtAction(nameof(GetById), new { id = newBusiness.BusinessID }, newBusiness);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, BusinessDto businessDto)
        {
            try
            {
                if (id != businessDto.BusinessID)
                {
                    return BadRequest("Business ID in the URL does not match the ID in the request body.");
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return Unauthorized("User ID claim is missing or invalid.");
                }

                var success = await _businessService.UpdateBusinessAsync(id, businessDto, userId);
                if (!success)
                    return NotFound(new { message = "Business not found or update failed." });

                return Ok(new { message = "Business updated successfully." });
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
                var success = await _businessService.DeleteBusinessAsync(id);
                if (!success)
                    return NotFound(new { message = "Business not found or could not be deleted." });

                return Ok(new { message = "Business deleted successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}