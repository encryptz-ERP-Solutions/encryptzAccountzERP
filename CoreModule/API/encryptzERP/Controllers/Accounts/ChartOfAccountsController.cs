using BusinessLogic.Accounts;
using BusinessLogic.Accounts.DTOs;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace encryptzERP.Controllers.Accounts
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChartOfAccountsController : ControllerBase
    {
        private readonly IChartOfAccountService _chartOfAccountService;
        private readonly ExceptionHandler _exceptionHandler;

        public ChartOfAccountsController(IChartOfAccountService chartOfAccountService, ExceptionHandler exceptionHandler)
        {
            _chartOfAccountService = chartOfAccountService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet("business/{businessId}")]
        public async Task<ActionResult<IEnumerable<ChartOfAccountDto>>> GetAll(Guid businessId)
        {
            try
            {
                var result = await _chartOfAccountService.GetAllChartOfAccountsAsync(businessId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChartOfAccountDto>> GetById(Guid id)
        {
            try
            {
                var result = await _chartOfAccountService.GetChartOfAccountByIdAsync(id);
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
        public async Task<IActionResult> Create(CreateChartOfAccountDto createDto)
        {
            try
            {
                var newChartOfAccount = await _chartOfAccountService.CreateChartOfAccountAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = newChartOfAccount.AccountID }, newChartOfAccount);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateChartOfAccountDto updateDto)
        {
            try
            {
                await _chartOfAccountService.UpdateChartOfAccountAsync(id, updateDto);
                return Ok(new { message = "Chart of account updated successfully." });
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
                await _chartOfAccountService.DeleteChartOfAccountAsync(id);
                return Ok(new { message = "Chart of account deleted successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
