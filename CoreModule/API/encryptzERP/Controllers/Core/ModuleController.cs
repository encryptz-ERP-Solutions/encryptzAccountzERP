using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace encryptzERP.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        private readonly ExceptionHandler _exceptionHandler;

        public ModuleController(IModuleService moduleService, ExceptionHandler exceptionHandler)
        {
            _moduleService = moduleService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModuleDto>>> GetAll()
        {
            try
            {
                var result = await _moduleService.GetAllModulesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ModuleDto>> GetById(int id)
        {
            try
            {
                var result = await _moduleService.GetModuleByIdAsync(id);
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
        public async Task<IActionResult> Create(ModuleDto moduleDto)
        {
            try
            {
                var newModule = await _moduleService.AddModuleAsync(moduleDto);
                return CreatedAtAction(nameof(GetById), new { id = newModule.ModuleID }, newModule);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ModuleDto moduleDto)
        {
            try
            {
                if (id != moduleDto.ModuleID)
                {
                    return BadRequest("Module ID in the URL does not match the ID in the request body.");
                }

                var success = await _moduleService.UpdateModuleAsync(id, moduleDto);
                if (!success)
                    return NotFound(new { message = "Module not found or update failed." });

                return Ok(new { message = "Module updated successfully." });
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
                var success = await _moduleService.DeleteModuleAsync(id);
                if (!success)
                    return NotFound(new { message = "Module not found or could not be deleted." });

                return Ok(new { message = "Module deleted successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}