using System;
using System.Threading.Tasks;
using BusinessLogic.Admin.DTOs;
using BusinessLogic.Admin.Interface;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace encryptzERP.Controllers.Admin
{
    [Route("api/admin/dashboard")]
    [ApiController]
    [Authorize]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _dashboardService;
        private readonly ExceptionHandler _exceptionHandler;

        public AdminDashboardController(IAdminDashboardService dashboardService, ExceptionHandler exceptionHandler)
        {
            _dashboardService = dashboardService;
            _exceptionHandler = exceptionHandler;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(AdminDashboardSummaryDto), 200)]
        public async Task<ActionResult<AdminDashboardSummaryDto>> GetSummary([FromQuery] int recentLimit = 5)
        {
            try
            {
                var limit = Math.Clamp(recentLimit, 1, 15);
                var response = await _dashboardService.GetSummaryAsync(limit);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "Failed to load admin dashboard summary." });
            }
        }
    }
}

