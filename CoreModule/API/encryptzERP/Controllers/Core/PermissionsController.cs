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
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly ExceptionHandler _exceptionHandler;

        public PermissionsController(IPermissionService permissionService, ExceptionHandler exceptionHandler)
        {
            _permissionService = permissionService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAll()
        {
            try
            {
                var result = await _permissionService.GetAllPermissionsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionDto>> GetById(int id)
        {
            try
            {
                var result = await _permissionService.GetPermissionByIdAsync(id);
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
        public async Task<IActionResult> Create(PermissionDto permissionDto)
        {
            try
            {
                var newPermission = await _permissionService.AddPermissionAsync(permissionDto);
                return CreatedAtAction(nameof(GetById), new { id = newPermission.PermissionID }, newPermission);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, PermissionDto permissionDto)
        {
            try
            {
                if (id != permissionDto.PermissionID)
                {
                    return BadRequest("Permission ID in the URL does not match the ID in the request body.");
                }

                var success = await _permissionService.UpdatePermissionAsync(id, permissionDto);
                if (!success)
                    return NotFound(new { message = "Permission not found or update failed." });

                return Ok(new { message = "Permission updated successfully." });
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
                var success = await _permissionService.DeletePermissionAsync(id);
                if (!success)
                    return NotFound(new { message = "Permission not found or could not be deleted." });

                return Ok(new { message = "Permission deleted successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
