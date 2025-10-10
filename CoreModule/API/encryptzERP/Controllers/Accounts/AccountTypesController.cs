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
    public class AccountTypesController : ControllerBase
    {
        private readonly IAccountTypeService _accountTypeService;
        private readonly ExceptionHandler _exceptionHandler;

        public AccountTypesController(IAccountTypeService accountTypeService, ExceptionHandler exceptionHandler)
        {
            _accountTypeService = accountTypeService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountTypeDto>>> GetAll()
        {
            try
            {
                var result = await _accountTypeService.GetAllAccountTypesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AccountTypeDto>> GetById(int id)
        {
            try
            {
                var result = await _accountTypeService.GetAccountTypeByIdAsync(id);
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
        public async Task<IActionResult> Create(CreateAccountTypeDto createDto)
        {
            try
            {
                var newAccountType = await _accountTypeService.CreateAccountTypeAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = newAccountType.AccountTypeID }, newAccountType);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateAccountTypeDto updateDto)
        {
            try
            {
                await _accountTypeService.UpdateAccountTypeAsync(id, updateDto);
                return Ok(new { message = "Account type updated successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _accountTypeService.DeleteAccountTypeAsync(id);
                return Ok(new { message = "Account type deleted successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
