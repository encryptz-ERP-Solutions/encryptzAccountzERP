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
        private readonly IUserService _UserService;
        private readonly ExceptionHandler _exceptionHandler;

        public UserController(IUserService UserService, ExceptionHandler exceptionHandler)
        {
            _UserService = UserService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            try
            {
                var result = await _UserService.GetAllUserAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {

                _exceptionHandler.LogError(ex);
                throw;
            }

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(long id)
        {
            try
            {
                var result = await _UserService.GetUserByIdAsync(id);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> Create(UserDto UserDto)
        {
            try
            {
                var response = await _UserService.AddUserAsync(UserDto);
                if (response == null)
                    return BadRequest("Failed to add business.");

                return Ok(new { message = "Business added successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, UserDto UserDto)
        {
            try
            {
                var success = await _UserService.UpdateUserAsync(id, UserDto);
                if (!success)
                    return NotFound(new { message = "User not found or update failed." });

                return Ok(new { message = "User updated successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var success = await _UserService.DeleteUserAsync(id);
                if (!success)
                    return NotFound(new { message = "User not found or could not be deleted." });

                return Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }

        }
    }
}
