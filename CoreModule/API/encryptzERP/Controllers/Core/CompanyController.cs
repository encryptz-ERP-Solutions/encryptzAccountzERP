using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Core.Interface;

namespace encryptzERP.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly ExceptionHandler _exceptionHandler;

        public CompanyController(ICompanyService companyService, ExceptionHandler exceptionHandler)
        {
            _companyService = companyService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetAll()
        {
            try
            {
                return Ok(await _companyService.GetAllCompanyAsync());
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetById(long id)
        {
            try
            {
                var Company = await _companyService.GetCompanyByIdAsync(id);
                if (Company == null) return NotFound();
                return Ok(Company);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> Create(CompanyDto Company)
        {
            try
            {
                await _companyService.AddCompanyAsync(Company);
                return CreatedAtAction(nameof(GetById), new { id = 0 }, Company);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, CompanyDto Company)
        {
            try
            {
                if (id <= 0) return BadRequest();
                await _companyService.UpdateCompanyAsync(Company);
                return NoContent();
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await _companyService.DeleteCompanyAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }
    }
}
