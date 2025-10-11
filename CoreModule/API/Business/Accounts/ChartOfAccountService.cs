using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Accounts.DTOs;
using Entities.Accounts;
using Repository.Accounts;

namespace BusinessLogic.Accounts
{
    public class ChartOfAccountService : IChartOfAccountService
    {
        private readonly IChartOfAccountRepository _chartOfAccountRepository;
        private readonly IMapper _mapper;

        public ChartOfAccountService(IChartOfAccountRepository chartOfAccountRepository, IMapper mapper)
        {
            _chartOfAccountRepository = chartOfAccountRepository;
            _mapper = mapper;
        }

        public async Task<ChartOfAccountDto> CreateChartOfAccountAsync(CreateChartOfAccountDto createChartOfAccountDto)
        {
            var chartOfAccount = _mapper.Map<ChartOfAccount>(createChartOfAccountDto);
            chartOfAccount.AccountID = Guid.NewGuid();
            chartOfAccount.CreatedAtUTC = DateTime.UtcNow;
            await _chartOfAccountRepository.AddChartOfAccountAsync(chartOfAccount);
            return _mapper.Map<ChartOfAccountDto>(chartOfAccount);
        }

        public async Task DeleteChartOfAccountAsync(Guid id)
        {
            await _chartOfAccountRepository.DeleteChartOfAccountAsync(id);
        }

        public async Task<IEnumerable<ChartOfAccountDto>> GetAllChartOfAccountsAsync(Guid businessId)
        {
            var chartOfAccounts = await _chartOfAccountRepository.GetAllChartOfAccountsAsync(businessId);
            return _mapper.Map<IEnumerable<ChartOfAccountDto>>(chartOfAccounts);
        }

        public async Task<ChartOfAccountDto> GetChartOfAccountByIdAsync(Guid id)
        {
            var chartOfAccount = await _chartOfAccountRepository.GetChartOfAccountByIdAsync(id);
            return _mapper.Map<ChartOfAccountDto>(chartOfAccount);
        }

        public async Task UpdateChartOfAccountAsync(Guid id, UpdateChartOfAccountDto updateChartOfAccountDto)
        {
            var chartOfAccount = await _chartOfAccountRepository.GetChartOfAccountByIdAsync(id);
            if (chartOfAccount == null)
            {
                return;
            }
            _mapper.Map(updateChartOfAccountDto, chartOfAccount);
            chartOfAccount.UpdatedAtUTC = DateTime.UtcNow;
            await _chartOfAccountRepository.UpdateChartOfAccountAsync(chartOfAccount);
        }
    }
}
