using BusinessLogic.Accounts;
using BusinessLogic.Accounts.DTOs;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace encryptzERP.Controllers.Accounts
{
    [Route("api/v1/businesses/{businessId}/vouchers")]
    [ApiController]
    [Authorize]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _voucherService;
        private readonly ExceptionHandler _exceptionHandler;

        public VouchersController(IVoucherService voucherService, ExceptionHandler exceptionHandler)
        {
            _voucherService = voucherService;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Get all vouchers for a business with optional filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VoucherDto>>> GetVouchers(
            Guid businessId,
            [FromQuery] string? status = null,
            [FromQuery] string? voucherType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var vouchers = await _voucherService.GetVouchersByBusinessIdAsync(
                    businessId, status, voucherType, startDate, endDate);
                return Ok(vouchers);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Get a specific voucher by ID
        /// </summary>
        [HttpGet("{voucherId}")]
        public async Task<ActionResult<VoucherDto>> GetVoucherById(Guid businessId, Guid voucherId)
        {
            try
            {
                var voucher = await _voucherService.GetVoucherByIdAsync(voucherId);
                if (voucher == null)
                    return NotFound(new { message = "Voucher not found" });

                // Verify voucher belongs to the specified business
                if (voucher.BusinessID != businessId)
                    return BadRequest(new { message = "Voucher does not belong to the specified business" });

                return Ok(voucher);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Create a new draft voucher
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<VoucherDto>> CreateVoucher(Guid businessId, [FromBody] CreateVoucherDto createDto)
        {
            try
            {
                // Ensure the business ID in the route matches the DTO
                if (createDto.BusinessID != businessId)
                    return BadRequest(new { message = "Business ID in route does not match the request body" });

                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized(new { message = "User ID claim is missing or invalid" });

                var voucher = await _voucherService.CreateVoucherAsync(createDto, userId);
                return CreatedAtAction(
                    nameof(GetVoucherById),
                    new { businessId = voucher.BusinessID, voucherId = voucher.VoucherID },
                    voucher);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Update an existing draft voucher
        /// </summary>
        [HttpPut("{voucherId}")]
        public async Task<ActionResult<VoucherDto>> UpdateVoucher(
            Guid businessId,
            Guid voucherId,
            [FromBody] UpdateVoucherDto updateDto)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized(new { message = "User ID claim is missing or invalid" });

                // Verify voucher exists and belongs to business
                var existingVoucher = await _voucherService.GetVoucherByIdAsync(voucherId);
                if (existingVoucher == null)
                    return NotFound(new { message = "Voucher not found" });

                if (existingVoucher.BusinessID != businessId)
                    return BadRequest(new { message = "Voucher does not belong to the specified business" });

                var voucher = await _voucherService.UpdateVoucherAsync(voucherId, updateDto, userId);
                return Ok(voucher);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Delete a draft voucher
        /// </summary>
        [HttpDelete("{voucherId}")]
        public async Task<IActionResult> DeleteVoucher(Guid businessId, Guid voucherId)
        {
            try
            {
                // Verify voucher exists and belongs to business
                var existingVoucher = await _voucherService.GetVoucherByIdAsync(voucherId);
                if (existingVoucher == null)
                    return NotFound(new { message = "Voucher not found" });

                if (existingVoucher.BusinessID != businessId)
                    return BadRequest(new { message = "Voucher does not belong to the specified business" });

                var success = await _voucherService.DeleteVoucherAsync(voucherId);
                if (!success)
                    return NotFound(new { message = "Voucher not found or could not be deleted" });

                return Ok(new { message = "Voucher deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        /// <summary>
        /// Post a draft voucher (marks as posted and triggers ledger generation stub)
        /// </summary>
        [HttpPost("{voucherId}/post")]
        public async Task<ActionResult<PostVoucherResponseDto>> PostVoucher(Guid businessId, Guid voucherId)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized(new { message = "User ID claim is missing or invalid" });

                // Verify voucher exists and belongs to business
                var existingVoucher = await _voucherService.GetVoucherByIdAsync(voucherId);
                if (existingVoucher == null)
                    return NotFound(new { message = "Voucher not found" });

                if (existingVoucher.BusinessID != businessId)
                    return BadRequest(new { message = "Voucher does not belong to the specified business" });

                var result = await _voucherService.PostVoucherAsync(voucherId, userId);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
