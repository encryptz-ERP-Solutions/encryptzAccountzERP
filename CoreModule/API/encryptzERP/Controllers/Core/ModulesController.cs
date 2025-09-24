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
    public class ModulesController : ControllerBase
    {
        private readonly IModulesService _modulesService;

        public ModulesController(IModulesService modulesService)
        {
            _modulesService = modulesService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Modules>>> GetAll()
        {
            return Ok(await _modulesService.GetAllModulesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Modules>> GetById(int id)
        {
            var modules = await _modulesService.GetModulesByIdAsync(id);
            if (modules == null) return NotFound();
            return Ok(modules);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ModulesDto modules)
        {
            await _modulesService.AddModulesAsync(modules);
            return CreatedAtAction(nameof(GetById), new { id = 0 }, modules);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ModulesDto modules)
        {
            if (id <= 0) return BadRequest();
            await _modulesService.UpdateModulesAsync(id, modules);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _modulesService.DeleteModulesAsync(id);
            return NoContent();
        }
    }
}
