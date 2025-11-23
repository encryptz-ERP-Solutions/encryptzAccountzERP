using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace encryptzERP.Controllers.Core
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserBusinessService _userBusinessService;

        public UsersController(IUserBusinessService userBusinessService)
        {
            _userBusinessService = userBusinessService;
        }

        [HttpGet("{userId}/default-business")]
        public async Task<ActionResult<Guid?>> GetDefaultBusiness(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                {
                    return BadRequest("userId is required");
                }

                var businessId = await _userBusinessService.GetDefaultBusinessIdAsync(userId);
                return Ok(new { businessId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}

