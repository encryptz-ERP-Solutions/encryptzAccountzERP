using System.Threading.Tasks;
using BusinessLogic.Admin.DTOs;

namespace BusinessLogic.Admin.Interface
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardSummaryDto> GetSummaryAsync(int recentLimit = 5);
    }
}

