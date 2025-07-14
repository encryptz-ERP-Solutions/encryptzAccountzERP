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
        private readonly IModulesService _ModulesService;
        private readonly ExceptionHandler _exceptionHandler;

        public ModulesController(IModulesService ModulesService, ExceptionHandler exceptionHandler)
        {
            _ModulesService = ModulesService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Modules>>> GetAll()
        {
            try
            {
                return Ok(await _ModulesService.GetAllModulesAsync());
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Modules>> GetById(int id)
        {
            try
            {
                var Modules = await _ModulesService.GetModulesByIdAsync(id);
                if (Modules == null) return NotFound();
                return Ok(Modules);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> Create(ModulesDto Modules)
        {
            try
            {
                await _ModulesService.AddModulesAsync(Modules);
                return CreatedAtAction(nameof(GetById), new { id = 0 }, Modules);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ModulesDto Modules)
        {
            try
            {
                if (id <= 0) return BadRequest();
                await _ModulesService.UpdateModulesAsync(id, Modules);
                return NoContent();
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _ModulesService.DeleteModulesAsync(id);
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
