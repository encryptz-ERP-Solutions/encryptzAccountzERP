using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Core.Interface;

namespace encryptzERP.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MenusController : ControllerBase
    {
        private readonly IMenusService _menusService;

        public MenusController(IMenusService menusService)
        {
            _menusService = menusService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Menus>>> GetAll()
        {
            return Ok(await _menusService.GetAllMenusAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Menus>> GetById(int id)
        {
            var menus = await _menusService.GetMenusByIdAsync(id);
            if (menus == null) return NotFound();
            return Ok(menus);
        }

        [HttpPost]
        public async Task<IActionResult> Create(MenusDto menus)
        {
            await _menusService.AddMenusAsync(menus);
            return CreatedAtAction(nameof(GetById), new { id = 0 }, menus);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MenusDto menus)
        {
            if (id <= 0) return BadRequest();
            await _menusService.UpdateMenusAsync(id, menus);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _menusService.DeleteMenusAsync(id);
            return NoContent();
        }
    }
}
