using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace encryptzERP.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;

        public ModuleController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModuleDto>>> GetAll()
        {
            var modules = await _moduleService.GetAllModulesAsync();
            return Ok(modules);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ModuleDto>> GetById(int id)
        {
            var module = await _moduleService.GetModuleByIdAsync(id);
            if (module == null)
            {
                return NotFound();
            }
            return Ok(module);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ModuleCreateDto moduleCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newModule = await _moduleService.CreateModuleAsync(moduleCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = newModule.ModuleID }, newModule);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ModuleUpdateDto moduleUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _moduleService.UpdateModuleAsync(id, moduleUpdateDto);
            if (!success)
            {
                return NotFound(new { message = "Module not found or update failed." });
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _moduleService.DeleteModuleAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Module not found or could not be deleted (system modules cannot be deleted)." });
            }
            return NoContent();
        }
    }
}