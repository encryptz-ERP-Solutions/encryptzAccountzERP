using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Accounts.DTOs;

namespace BusinessLogic.Accounts
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionHeaderDto>> GetTransactionsByBusinessIdAsync(Guid businessId);
        Task<TransactionHeaderDto> GetTransactionByIdAsync(Guid transactionHeaderId);
        Task<TransactionHeaderDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto);
        Task UpdateTransactionHeaderAsync(Guid transactionHeaderId, UpdateTransactionHeaderDto updateDto);
        Task DeleteTransactionAsync(Guid transactionHeaderId);
    }
}
