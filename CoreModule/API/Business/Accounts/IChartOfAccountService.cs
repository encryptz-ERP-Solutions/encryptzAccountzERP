using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Accounts.DTOs;

namespace BusinessLogic.Accounts
{
    public interface IChartOfAccountService
    {
        Task<IEnumerable<ChartOfAccountDto>> GetAllChartOfAccountsAsync(Guid businessId);
        Task<ChartOfAccountDto> GetChartOfAccountByIdAsync(Guid id);
        Task<ChartOfAccountDto> CreateChartOfAccountAsync(CreateChartOfAccountDto createChartOfAccountDto);
        Task UpdateChartOfAccountAsync(Guid id, UpdateChartOfAccountDto updateChartOfAccountDto);
        Task DeleteChartOfAccountAsync(Guid id);
    }
}
