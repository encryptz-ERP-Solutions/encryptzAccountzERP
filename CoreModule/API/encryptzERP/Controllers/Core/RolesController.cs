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
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ExceptionHandler _exceptionHandler;

        public RolesController(IRoleService roleService, ExceptionHandler exceptionHandler)
        {
            _roleService = roleService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
        {
            try
            {
                var result = await _roleService.GetAllRolesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetById(int id)
        {
            try
            {
                var result = await _roleService.GetRoleByIdAsync(id);
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
        public async Task<IActionResult> Create(RoleDto roleDto)
        {
            try
            {
                var newRole = await _roleService.AddRoleAsync(roleDto);
                return CreatedAtAction(nameof(GetById), new { id = newRole.RoleID }, newRole);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RoleDto roleDto)
        {
            try
            {
                if (id != roleDto.RoleID)
                {
                    return BadRequest("Role ID in the URL does not match the ID in the request body.");
                }

                var success = await _roleService.UpdateRoleAsync(id, roleDto);
                if (!success)
                    return NotFound(new { message = "Role not found or update failed." });

                return Ok(new { message = "Role updated successfully." });
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
                var success = await _roleService.DeleteRoleAsync(id);
                if (!success)
                    return NotFound(new { message = "Role not found or could not be deleted." });

                return Ok(new { message = "Role deleted successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
