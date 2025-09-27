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
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RoleDto>> GetById(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleCreateDto roleCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newRole = await _roleService.CreateRoleAsync(roleCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = newRole.RoleID }, newRole);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] RoleUpdateDto roleUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _roleService.UpdateRoleAsync(id, roleUpdateDto);
            if (!success)
            {
                return NotFound(new { message = "Role not found or update failed." });
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _roleService.DeleteRoleAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Role not found or could not be deleted (system roles cannot be deleted)." });
            }
            return NoContent();
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleDto assignRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _roleService.AssignRoleToUserAsync(assignRoleDto);
            if (!success)
            {
                return NotFound(new { message = "Assignment failed. Ensure the user, business, and role all exist." });
            }

            return Ok(new { message = "Role assigned successfully." });
        }
    }
}