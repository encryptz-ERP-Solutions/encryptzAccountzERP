using System;
using System.Threading.Tasks;
using BusinessLogic.Accounts;
using BusinessLogic.Accounts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace encryptzERP.Controllers.Accounts
{
    [Authorize]
    [ApiController]
    [Route("api/v1/businesses/{businessId}/ledgers")]
    public class LedgerController : ControllerBase
    {
        private readonly ILedgerService _ledgerService;

        public LedgerController(ILedgerService ledgerService)
        {
            _ledgerService = ledgerService;
        }

        /// <summary>
        /// Get ledger statement for a specific account
        /// </summary>
        /// <param name="businessId">Business ID</param>
        /// <param name="accountId">Account ID</param>
        /// <param name="from">Start date (YYYY-MM-DD)</param>
        /// <param name="to">End date (YYYY-MM-DD)</param>
        /// <returns>Ledger statement with entries and balances</returns>
        [HttpGet("{accountId}")]
        public async Task<ActionResult<LedgerStatementDto>> GetLedgerStatement(
            Guid businessId,
            Guid accountId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var fromDate = from ?? new DateTime(DateTime.UtcNow.Year, 4, 1); // Default: Current financial year start (April 1)
                var toDate = to ?? DateTime.UtcNow.Date;

                var statement = await _ledgerService.GetLedgerStatementAsync(accountId, fromDate, toDate);
                
                // Validate business ID matches
                if (statement.AccountID != Guid.Empty && businessId != Guid.Empty)
                {
                    // This would require additional validation - for now we trust the account belongs to business
                }

                return Ok(statement);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the ledger statement", error = ex.Message });
            }
        }

        /// <summary>
        /// Post a voucher to the ledger (manual posting if automatic posting failed)
        /// </summary>
        /// <param name="businessId">Business ID</param>
        /// <param name="voucherId">Voucher ID to post</param>
        /// <returns>Post result with entry count</returns>
        [HttpPost("post/{voucherId}")]
        public async Task<ActionResult<PostVoucherToLedgerDto>> PostVoucherToLedger(
            Guid businessId,
            Guid voucherId)
        {
            try
            {
                // Get user ID from claims
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { message = "User not authenticated" });

                var userId = Guid.Parse(userIdClaim);

                var result = await _ledgerService.PostVoucherToLedgerAsync(voucherId, userId);
                
                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while posting to ledger", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a voucher has been posted to ledger
        /// </summary>
        /// <param name="businessId">Business ID</param>
        /// <param name="voucherId">Voucher ID</param>
        /// <returns>Boolean indicating if ledger entries exist</returns>
        [HttpGet("check/{voucherId}")]
        public async Task<ActionResult<object>> CheckVoucherPosted(
            Guid businessId,
            Guid voucherId)
        {
            try
            {
                var hasEntries = await _ledgerService.HasLedgerEntriesAsync(voucherId);
                return Ok(new { voucherId, hasLedgerEntries = hasEntries });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking ledger status", error = ex.Message });
            }
        }
    }
}
