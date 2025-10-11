using Microsoft.AspNetCore.Mvc;
using Shared.Core;
using System.Threading.Tasks;
using BusinessLogic.Core.Interface;

namespace encryptzERP.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolePermissionsController : ControllerBase
    {
        private readonly IRolePermissionService _rolePermissionService;

        public RolePermissionsController(IRolePermissionService rolePermissionService)
        {
            _rolePermissionService = rolePermissionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _rolePermissionService.GetAllAsync());
        }

        [HttpGet("{roleId}/{permissionId}")]
        public async Task<IActionResult> GetById(int roleId, int permissionId)
        {
            var result = await _rolePermissionService.GetByIdAsync(roleId, permissionId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RolePermissionDto rolePermissionDto)
        {
            await _rolePermissionService.AddAsync(rolePermissionDto);
            return CreatedAtAction(nameof(GetById), new { roleId = rolePermissionDto.RoleID, permissionId = rolePermissionDto.PermissionID }, rolePermissionDto);
        }

        [HttpDelete("{roleId}/{permissionId}")]
        public async Task<IActionResult> Delete(int roleId, int permissionId)
        {
            await _rolePermissionService.DeleteAsync(roleId, permissionId);
            return NoContent();
        }
    }
}
