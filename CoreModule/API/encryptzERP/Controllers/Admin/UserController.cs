using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Admin.DTOs;
using BusinessLogic.Admin.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace encryptzERP.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Secure all endpoints in this controller
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var result = await _userService.GetAllUsersAsync();
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            var result = await _userService.GetUserByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Consider adding a check to see if user handle or email already exists
            var existingUserByHandle = await _userService.GetUserByUserHandleAsync(userCreateDto.UserHandle);
            if (existingUserByHandle != null)
            {
                return Conflict(new { Message = $"User handle '{userCreateDto.UserHandle}' is already taken." });
            }

            var existingUserByEmail = await _userService.GetUserByEmailAsync(userCreateDto.Email);
            if (existingUserByEmail != null)
            {
                 return Conflict(new { Message = $"Email '{userCreateDto.Email}' is already in use." });
            }

            var newUser = await _userService.CreateUserAsync(userCreateDto);

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.UserID }, newUser);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto userUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _userService.UpdateUserAsync(id, userUpdateDto);
            if (!success)
            {
                return NotFound(new { message = "User not found or update failed." });
            }

            return NoContent(); // Indicates success with no content to return
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound(new { message = "User not found or could not be deleted." });
            }

            return NoContent(); // Indicates success with no content to return
        }
    }
}