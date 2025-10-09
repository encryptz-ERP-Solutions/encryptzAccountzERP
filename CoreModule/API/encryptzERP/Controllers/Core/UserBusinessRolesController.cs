using Microsoft.AspNetCore.Mvc;
using Business.Core;
using Shared.Core;
using System.Threading.Tasks;
using System;

namespace encryptzERP.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserBusinessRolesController : ControllerBase
    {
        private readonly IUserBusinessRoleService _userBusinessRoleService;

        public UserBusinessRolesController(IUserBusinessRoleService userBusinessRoleService)
        {
            _userBusinessRoleService = userBusinessRoleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _userBusinessRoleService.GetAllAsync());
        }

        [HttpGet("{userId}/{businessId}/{roleId}")]
        public async Task<IActionResult> GetById(Guid userId, Guid businessId, int roleId)
        {
            var result = await _userBusinessRoleService.GetByIdAsync(userId, businessId, roleId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserBusinessRoleDto userBusinessRoleDto)
        {
            await _userBusinessRoleService.AddAsync(userBusinessRoleDto);
            return CreatedAtAction(nameof(GetById), new { userId = userBusinessRoleDto.UserID, businessId = userBusinessRoleDto.BusinessID, roleId = userBusinessRoleDto.RoleID }, userBusinessRoleDto);
        }

        [HttpDelete("{userId}/{businessId}/{roleId}")]
        public async Task<IActionResult> Delete(Guid userId, Guid businessId, int roleId)
        {
            await _userBusinessRoleService.DeleteAsync(userId, businessId, roleId);
            return NoContent();
        }
    }
}
