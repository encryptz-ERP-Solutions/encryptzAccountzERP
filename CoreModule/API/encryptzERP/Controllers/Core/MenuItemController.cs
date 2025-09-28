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
    public class MenuItemController : ControllerBase
    {
        private readonly IMenuItemService _menuItemService;
        private readonly ExceptionHandler _exceptionHandler;

        public MenuItemController(IMenuItemService menuItemService, ExceptionHandler exceptionHandler)
        {
            _menuItemService = menuItemService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetAll()
        {
            try
            {
                var result = await _menuItemService.GetAllMenuItemsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MenuItemDto>> GetById(int id)
        {
            try
            {
                var result = await _menuItemService.GetMenuItemByIdAsync(id);
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
        public async Task<IActionResult> Create(MenuItemDto menuItemDto)
        {
            try
            {
                var newMenuItem = await _menuItemService.AddMenuItemAsync(menuItemDto);
                return CreatedAtAction(nameof(GetById), new { id = newMenuItem.MenuItemID }, newMenuItem);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MenuItemDto menuItemDto)
        {
            try
            {
                if (id != menuItemDto.MenuItemID)
                {
                    return BadRequest("MenuItem ID in the URL does not match the ID in the request body.");
                }

                var success = await _menuItemService.UpdateMenuItemAsync(id, menuItemDto);
                if (!success)
                    return NotFound(new { message = "MenuItem not found or update failed." });

                return Ok(new { message = "MenuItem updated successfully." });
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
                var success = await _menuItemService.DeleteMenuItemAsync(id);
                if (!success)
                    return NotFound(new { message = "MenuItem not found or could not be deleted." });

                return Ok(new { message = "MenuItem deleted successfully." });
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}