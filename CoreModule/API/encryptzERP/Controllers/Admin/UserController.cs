using BusinessLogic.Admin.DTOs;
using BusinessLogic.Admin.Interface;
using Entities.Admin;
using Entities.Core;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Admin.Interface;
using Repository.Core.Interface;

namespace encryptzERP.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var result = await _userService.GetAllUserAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(long id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserDto userDto)
        {
            var response = await _userService.AddUserAsync(userDto);
            if (response == null)
                return BadRequest("Failed to add user.");

            return Ok(new { message = "User added successfully." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, UserDto userDto)
        {
            var success = await _userService.UpdateUserAsync(id, userDto);
            if (!success)
                return NotFound(new { message = "User not found or update failed." });

            return Ok(new { message = "User updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
                return NotFound(new { message = "User not found or could not be deleted." });

            return Ok(new { message = "User deleted successfully." });
        }
    }
}
