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
    [Route("api/v1/businesses/{businessId}/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly ILedgerService _ledgerService;

        public ReportsController(ILedgerService ledgerService)
        {
            _ledgerService = ledgerService;
        }

        /// <summary>
        /// Get Trial Balance report
        /// </summary>
        /// <param name="businessId">Business ID</param>
        /// <param name="from">Start date (YYYY-MM-DD)</param>
        /// <param name="to">End date (YYYY-MM-DD)</param>
        /// <returns>Trial Balance with opening, period, and closing balances</returns>
        [HttpGet("trial-balance")]
        public async Task<ActionResult<TrialBalanceDto>> GetTrialBalance(
            Guid businessId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var fromDate = from ?? new DateTime(DateTime.UtcNow.Year, 4, 1); // Default: Current financial year start (April 1)
                var toDate = to ?? DateTime.UtcNow.Date;

                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be after to date" });

                var trialBalance = await _ledgerService.GetTrialBalanceAsync(businessId, fromDate, toDate);
                return Ok(trialBalance);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the trial balance", error = ex.Message });
            }
        }

        /// <summary>
        /// Get Profit and Loss Statement
        /// </summary>
        /// <param name="businessId">Business ID</param>
        /// <param name="from">Start date (YYYY-MM-DD)</param>
        /// <param name="to">End date (YYYY-MM-DD)</param>
        /// <returns>P&L statement with income, expenses, and net profit</returns>
        [HttpGet("p-and-l")]
        [HttpGet("profit-and-loss")]
        public async Task<ActionResult<ProfitAndLossDto>> GetProfitAndLoss(
            Guid businessId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var fromDate = from ?? new DateTime(DateTime.UtcNow.Year, 4, 1); // Default: Current financial year start (April 1)
                var toDate = to ?? DateTime.UtcNow.Date;

                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be after to date" });

                var profitAndLoss = await _ledgerService.GetProfitAndLossAsync(businessId, fromDate, toDate);
                return Ok(profitAndLoss);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the P&L statement", error = ex.Message });
            }
        }

        /// <summary>
        /// Get reconciliation check - validates that debits equal credits
        /// </summary>
        /// <param name="businessId">Business ID</param>
        /// <param name="from">Start date (YYYY-MM-DD)</param>
        /// <param name="to">End date (YYYY-MM-DD)</param>
        /// <returns>Reconciliation status with any unbalanced vouchers</returns>
        [HttpGet("reconciliation-check")]
        public async Task<ActionResult<ReconciliationCheckDto>> GetReconciliationCheck(
            Guid businessId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var fromDate = from ?? new DateTime(DateTime.UtcNow.Year, 4, 1); // Default: Current financial year start (April 1)
                var toDate = to ?? DateTime.UtcNow.Date;

                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be after to date" });

                var reconciliation = await _ledgerService.GetReconciliationCheckAsync(businessId, fromDate, toDate);
                return Ok(reconciliation);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while performing reconciliation check", error = ex.Message });
            }
        }

        /// <summary>
        /// Get summary of all financial reports
        /// </summary>
        /// <param name="businessId">Business ID</param>
        /// <param name="from">Start date (YYYY-MM-DD)</param>
        /// <param name="to">End date (YYYY-MM-DD)</param>
        /// <returns>Summary dashboard with key metrics</returns>
        [HttpGet("summary")]
        public async Task<ActionResult<object>> GetReportsSummary(
            Guid businessId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var fromDate = from ?? new DateTime(DateTime.UtcNow.Year, 4, 1); // Default: Current financial year start (April 1)
                var toDate = to ?? DateTime.UtcNow.Date;

                if (fromDate > toDate)
                    return BadRequest(new { message = "From date cannot be after to date" });

                // Get all reports in parallel
                var trialBalanceTask = _ledgerService.GetTrialBalanceAsync(businessId, fromDate, toDate);
                var profitAndLossTask = _ledgerService.GetProfitAndLossAsync(businessId, fromDate, toDate);
                var reconciliationTask = _ledgerService.GetReconciliationCheckAsync(businessId, fromDate, toDate);

                await Task.WhenAll(trialBalanceTask, profitAndLossTask, reconciliationTask);

                var trialBalance = await trialBalanceTask;
                var profitAndLoss = await profitAndLossTask;
                var reconciliation = await reconciliationTask;

                var summary = new
                {
                    businessId,
                    businessName = trialBalance.BusinessName,
                    fromDate,
                    toDate,
                    trialBalance = new
                    {
                        totalDebits = trialBalance.TotalClosingDebit,
                        totalCredits = trialBalance.TotalClosingCredit,
                        isBalanced = trialBalance.IsBalanced,
                        difference = trialBalance.Difference
                    },
                    profitAndLoss = new
                    {
                        totalIncome = profitAndLoss.TotalIncome,
                        totalExpenses = profitAndLoss.TotalExpenses,
                        netProfit = profitAndLoss.NetProfit,
                        isProfitable = profitAndLoss.IsProfitable
                    },
                    reconciliation = new
                    {
                        totalEntries = reconciliation.TotalEntries,
                        totalVouchers = reconciliation.TotalVouchers,
                        isBalanced = reconciliation.IsBalanced,
                        unbalancedVoucherCount = reconciliation.UnbalancedVouchers.Count
                    }
                };

                return Ok(summary);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating reports summary", error = ex.Message });
            }
        }
    }
}
