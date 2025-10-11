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
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ExceptionHandler _exceptionHandler;

        public TransactionsController(ITransactionService transactionService, ExceptionHandler exceptionHandler)
        {
            _transactionService = transactionService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet("business/{businessId}")]
        public async Task<ActionResult<IEnumerable<TransactionHeaderDto>>> GetAll(Guid businessId)
        {
            try
            {
                var result = await _transactionService.GetTransactionsByBusinessIdAsync(businessId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionHeaderDto>> GetById(Guid id)
        {
            try
            {
                var result = await _transactionService.GetTransactionByIdAsync(id);
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
        public async Task<IActionResult> Create(CreateTransactionDto createDto)
        {
            try
            {
                var newTransaction = await _transactionService.CreateTransactionAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = newTransaction.TransactionHeaderID }, newTransaction);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateTransactionHeaderDto updateDto)
        {
            try
            {
                await _transactionService.UpdateTransactionHeaderAsync(id, updateDto);
                return Ok(new { message = "Transaction header updated successfully." });
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
                await _transactionService.DeleteTransactionAsync(id);
                return Ok(new { message = "Transaction deleted successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
