using BusinessLogic.Core.DTOs;
using System;
using System.Threading.Tasks;

namespace BusinessLogic.Core.Interface
{
    /// <summary>
    /// Service interface for dashboard operations
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Get dashboard data for a specific business including KPIs, recent activities, and subscription status
        /// </summary>
        /// <param name="businessId">The business ID to get dashboard data for</param>
        /// <param name="request">Request parameters including pagination settings</param>
        /// <returns>Dashboard response with all relevant data</returns>
        Task<DashboardResponseDto> GetDashboardDataAsync(Guid businessId, DashboardRequestDto request);

        /// <summary>
        /// Get KPIs for a specific business
        /// </summary>
        /// <param name="businessId">The business ID</param>
        /// <returns>Dashboard KPIs</returns>
        Task<DashboardKpisDto> GetKpisAsync(Guid businessId);

        /// <summary>
        /// Get recent activities from audit logs for a specific business
        /// </summary>
        /// <param name="businessId">The business ID</param>
        /// <param name="limit">Maximum number of activities to return</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of recent activities</returns>
        Task<System.Collections.Generic.List<RecentActivityDto>> GetRecentActivitiesAsync(Guid businessId, int limit, int offset);

        /// <summary>
        /// Get subscription status for a specific business
        /// </summary>
        /// <param name="businessId">The business ID</param>
        /// <returns>Subscription status information</returns>
        Task<SubscriptionStatusDto> GetSubscriptionStatusAsync(Guid businessId);
    }
}
