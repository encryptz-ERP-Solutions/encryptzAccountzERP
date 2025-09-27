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
    public class MenuItemController : ControllerBase
    {
        private readonly IMenuItemService _menuItemService;

        public MenuItemController(IMenuItemService menuItemService)
        {
            _menuItemService = menuItemService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetAll()
        {
            var menuItems = await _menuItemService.GetAllMenuItemsAsync();
            return Ok(menuItems);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MenuItemDto>> GetById(int id)
        {
            var menuItem = await _menuItemService.GetMenuItemByIdAsync(id);
            if (menuItem == null)
            {
                return NotFound();
            }
            return Ok(menuItem);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MenuItemCreateDto menuItemCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newMenuItem = await _menuItemService.CreateMenuItemAsync(menuItemCreateDto);
            return CreatedAtAction(nameof(GetById), new { id = newMenuItem.MenuItemID }, newMenuItem);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] MenuItemUpdateDto menuItemUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _menuItemService.UpdateMenuItemAsync(id, menuItemUpdateDto);
            if (!success)
            {
                return NotFound(new { message = "Menu item not found or update failed." });
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _menuItemService.DeleteMenuItemAsync(id);
            if (!success)
            {
                return NotFound(new { message = "Menu item not found or could not be deleted." });
            }
            return NoContent();
        }
    }
}