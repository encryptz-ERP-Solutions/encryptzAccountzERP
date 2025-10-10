using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Accounts;

namespace Repository.Accounts
{
    public interface IChartOfAccountRepository
    {
        Task<IEnumerable<ChartOfAccount>> GetAllChartOfAccountsAsync(Guid businessId);
        Task<ChartOfAccount> GetChartOfAccountByIdAsync(Guid id);
        Task AddChartOfAccountAsync(ChartOfAccount chartOfAccount);
        Task UpdateChartOfAccountAsync(ChartOfAccount chartOfAccount);
        Task DeleteChartOfAccountAsync(Guid id);
    }
}
