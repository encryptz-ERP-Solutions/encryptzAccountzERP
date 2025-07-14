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
        private readonly IMenusService _MenusService;
        private readonly ExceptionHandler _exceptionHandler;

        public MenusController(IMenusService MenusService, ExceptionHandler exceptionHandler)
        {
            _MenusService = MenusService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Menus>>> GetAll()
        {
            try
            {
                return Ok(await _MenusService.GetAllMenusAsync());
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Menus>> GetById(int id)
        {
            try
            {
                var Menus = await _MenusService.GetMenusByIdAsync(id);
                if (Menus == null) return NotFound();
                return Ok(Menus);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> Create(MenusDto Menus)
        {
            try
            {
                await _MenusService.AddMenusAsync(Menus);
                return CreatedAtAction(nameof(GetById), new { id = 0 }, Menus);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MenusDto Menus)
        {
            try
            {
                if (id <= 0) return BadRequest();
                await _MenusService.UpdateMenusAsync(id, Menus);
                return NoContent();
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _MenusService.DeleteMenusAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                throw;
            }
            
        }
    }
}
