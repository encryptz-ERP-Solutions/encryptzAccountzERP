using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace encryptzERP.Controllers.Core
{
    /// <summary>
    /// Controller for dashboard operations providing KPIs, recent activities, and business insights
    /// </summary>
    [Route("api/v1/businesses")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ExceptionHandler _exceptionHandler;

        public DashboardController(IDashboardService dashboardService, ExceptionHandler exceptionHandler)
        {
            _dashboardService = dashboardService;
            _exceptionHandler = exceptionHandler;
        }

        /// <summary>
        /// Get dashboard data for a specific business including KPIs, recent activities, shortcuts, and subscription status
        /// </summary>
        /// <param name="id">Business ID</param>
        /// <param name="limit">Number of recent activities to return (default: 20, max: 100)</param>
        /// <param name="offset">Offset for pagination of recent activities (default: 0)</param>
        /// <returns>Dashboard data with KPIs, recent activities, shortcuts, and subscription status</returns>
        /// <response code="200">Returns the dashboard data</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="404">If the business is not found</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpGet("{id}/dashboard")]
        [ProducesResponseType(typeof(DashboardResponseDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DashboardResponseDto>> GetDashboard(
            Guid id,
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            try
            {
                var request = new DashboardRequestDto
                {
                    Limit = limit,
                    Offset = offset
                };

                var result = await _dashboardService.GetDashboardDataAsync(id, request);
                
                if (result == null)
                {
                    return NotFound(new { message = $"Dashboard data not found for business {id}" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "An internal error occurred while fetching dashboard data." });
            }
        }

        /// <summary>
        /// Get only KPIs for a specific business (lightweight endpoint)
        /// </summary>
        /// <param name="id">Business ID</param>
        /// <returns>Dashboard KPIs</returns>
        /// <response code="200">Returns the KPIs</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpGet("{id}/dashboard/kpis")]
        [ProducesResponseType(typeof(DashboardKpisDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DashboardKpisDto>> GetKpis(Guid id)
        {
            try
            {
                var result = await _dashboardService.GetKpisAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "An internal error occurred while fetching KPIs." });
            }
        }

        /// <summary>
        /// Get recent activities for a specific business
        /// </summary>
        /// <param name="id">Business ID</param>
        /// <param name="limit">Number of activities to return (default: 20, max: 100)</param>
        /// <param name="offset">Offset for pagination (default: 0)</param>
        /// <returns>List of recent activities</returns>
        /// <response code="200">Returns the recent activities</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpGet("{id}/dashboard/activities")]
        [ProducesResponseType(typeof(System.Collections.Generic.List<RecentActivityDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<System.Collections.Generic.List<RecentActivityDto>>> GetRecentActivities(
            Guid id,
            [FromQuery] int limit = 20,
            [FromQuery] int offset = 0)
        {
            try
            {
                var result = await _dashboardService.GetRecentActivitiesAsync(id, limit, offset);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "An internal error occurred while fetching recent activities." });
            }
        }

        /// <summary>
        /// Get subscription status for a specific business
        /// </summary>
        /// <param name="id">Business ID</param>
        /// <returns>Subscription status</returns>
        /// <response code="200">Returns the subscription status</response>
        /// <response code="401">If the user is not authenticated</response>
        /// <response code="500">If an internal error occurs</response>
        [HttpGet("{id}/dashboard/subscription")]
        [ProducesResponseType(typeof(SubscriptionStatusDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<SubscriptionStatusDto>> GetSubscriptionStatus(Guid id)
        {
            try
            {
                var result = await _dashboardService.GetSubscriptionStatusAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _exceptionHandler.LogError(ex);
                return StatusCode(500, new { message = "An internal error occurred while fetching subscription status." });
            }
        }
    }
}
