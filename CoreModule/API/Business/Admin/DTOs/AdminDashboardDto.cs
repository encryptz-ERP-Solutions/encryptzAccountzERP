using System;
using System.Collections.Generic;

namespace BusinessLogic.Admin.DTOs
{
    public class AdminDashboardSummaryDto
    {
        public AdminDashboardSummaryDto()
        {
            Kpis = new AdminDashboardKpiDto();
            RecentUsers = new List<AdminUserSummaryDto>();
            RecentBusinesses = new List<AdminBusinessSummaryDto>();
            PlanUsage = new List<AdminPlanUsageDto>();
            RecentActivity = new List<AdminActivityDto>();
        }

        public AdminDashboardKpiDto Kpis { get; set; }
        public List<AdminUserSummaryDto> RecentUsers { get; set; }
        public List<AdminBusinessSummaryDto> RecentBusinesses { get; set; }
        public List<AdminPlanUsageDto> PlanUsage { get; set; }
        public List<AdminActivityDto> RecentActivity { get; set; }
    }

    public class AdminDashboardKpiDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalBusinesses { get; set; }
        public int ActiveBusinesses { get; set; }
        public int TotalRoles { get; set; }
        public int TotalPermissions { get; set; }
        public int TotalModules { get; set; }
        public int TotalMenuItems { get; set; }
        public int SubscriptionPlans { get; set; }
    }

    public class AdminUserSummaryDto
    {
        public Guid UserId { get; set; }
        public string UserHandle { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class AdminBusinessSummaryDto
    {
        public Guid BusinessId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? State { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class AdminPlanUsageDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int ActiveSubscriptions { get; set; }
        public bool IsActive { get; set; }
    }

    public class AdminActivityDto
    {
        public long AuditLogId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public string? ChangedByUserName { get; set; }
        public DateTime ChangedAtUtc { get; set; }
        public string? Description { get; set; }
    }
}

