using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Accounts;

namespace Repository.Accounts
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<TransactionHeader>> GetTransactionsByBusinessIdAsync(Guid businessId);
        Task<TransactionHeader> GetTransactionByIdAsync(Guid transactionHeaderId);
        Task<TransactionHeader> CreateTransactionAsync(TransactionHeader transactionHeader);
        Task UpdateTransactionHeaderAsync(TransactionHeader transactionHeader);
        Task DeleteTransactionAsync(Guid transactionHeaderId);
    }
}
