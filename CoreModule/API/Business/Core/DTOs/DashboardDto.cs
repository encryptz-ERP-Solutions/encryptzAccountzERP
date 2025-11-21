using System;
using System.Collections.Generic;

namespace BusinessLogic.Core.DTOs
{
    /// <summary>
    /// Dashboard response containing KPIs, recent activities, shortcuts, and subscription status
    /// </summary>
    public class DashboardResponseDto
    {
        public DashboardKpisDto Kpis { get; set; } = new();
        public List<RecentActivityDto> RecentActivities { get; set; } = new();
        public List<DashboardShortcutDto> Shortcuts { get; set; } = new();
        public SubscriptionStatusDto SubscriptionStatus { get; set; } = new();
    }

    /// <summary>
    /// KPI metrics for the dashboard
    /// </summary>
    public class DashboardKpisDto
    {
        /// <summary>
        /// Total accounts receivable amount
        /// </summary>
        public decimal Receivables { get; set; }

        /// <summary>
        /// Total accounts payable amount
        /// </summary>
        public decimal Payables { get; set; }

        /// <summary>
        /// Total cash balance
        /// </summary>
        public decimal Cash { get; set; }

        /// <summary>
        /// Total revenue for current period
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// Total expenses for current period
        /// </summary>
        public decimal Expenses { get; set; }

        /// <summary>
        /// Net profit (Revenue - Expenses)
        /// </summary>
        public decimal NetProfit { get; set; }
    }

    /// <summary>
    /// Recent activity item from audit logs
    /// </summary>
    public class RecentActivityDto
    {
        public long AuditLogId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? ChangedByUserName { get; set; }
        public DateTime ChangedAtUtc { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dashboard shortcut for quick navigation
    /// </summary>
    public class DashboardShortcutDto
    {
        public string Label { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Current subscription status for the business
    /// </summary>
    public class SubscriptionStatusDto
    {
        public string PlanName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int DaysRemaining { get; set; }
        public bool IsTrialActive { get; set; }
        public DateTime? TrialEndsAt { get; set; }
    }

    /// <summary>
    /// Request parameters for dashboard data with pagination
    /// </summary>
    public class DashboardRequestDto
    {
        /// <summary>
        /// Number of recent activities to return (default: 20, max: 100)
        /// </summary>
        public int Limit { get; set; } = 20;

        /// <summary>
        /// Offset for pagination of recent activities
        /// </summary>
        public int Offset { get; set; } = 0;
    }
}
