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

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetAll()
        {
            return Ok(await _companyService.GetAllCompanyAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetById(long id)
        {
            var Company = await _companyService.GetCompanyByIdAsync(id);
            if (Company == null) return NotFound();
            return Ok(Company);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CompanyDto Company)
        {
            await _companyService.AddCompanyAsync(Company);
            return CreatedAtAction(nameof(GetById), new { id = 0 }, Company);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, CompanyDto Company)
        {
            if (id <= 0) return BadRequest();
            await _companyService.UpdateCompanyAsync(Company);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _companyService.DeleteCompanyAsync(id);
            return NoContent();
        }
    }
}
