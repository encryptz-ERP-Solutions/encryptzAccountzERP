using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Accounts.DTOs;
using Entities.Accounts;
using Repository.Accounts;

namespace BusinessLogic.Accounts
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IMapper _mapper;

        public TransactionService(ITransactionRepository transactionRepository, IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _mapper = mapper;
        }

        public async Task<TransactionHeaderDto> CreateTransactionAsync(CreateTransactionDto createTransactionDto)
        {
            var transactionHeader = _mapper.Map<TransactionHeader>(createTransactionDto);

            transactionHeader.TransactionHeaderID = Guid.NewGuid();
            transactionHeader.CreatedAtUTC = DateTime.UtcNow;

            // Basic validation
            if (transactionHeader.TransactionDetails == null || !transactionHeader.TransactionDetails.Any())
            {
                throw new ArgumentException("Transaction must have at least one detail line.");
            }

            decimal totalDebit = transactionHeader.TransactionDetails.Sum(d => d.DebitAmount);
            decimal totalCredit = transactionHeader.TransactionDetails.Sum(d => d.CreditAmount);

            if (totalDebit != totalCredit)
            {
                throw new InvalidOperationException("Total debits must equal total credits.");
            }

            var createdTransaction = await _transactionRepository.CreateTransactionAsync(transactionHeader);
            return await GetTransactionByIdAsync(createdTransaction.TransactionHeaderID);
        }

        public async Task DeleteTransactionAsync(Guid transactionHeaderId)
        {
            await _transactionRepository.DeleteTransactionAsync(transactionHeaderId);
        }

        public async Task<TransactionHeaderDto> GetTransactionByIdAsync(Guid transactionHeaderId)
        {
            var transaction = await _transactionRepository.GetTransactionByIdAsync(transactionHeaderId);
            return _mapper.Map<TransactionHeaderDto>(transaction);
        }

        public async Task<IEnumerable<TransactionHeaderDto>> GetTransactionsByBusinessIdAsync(Guid businessId)
        {
            var transactions = await _transactionRepository.GetTransactionsByBusinessIdAsync(businessId);
            return _mapper.Map<IEnumerable<TransactionHeaderDto>>(transactions);
        }

        public async Task UpdateTransactionHeaderAsync(Guid transactionHeaderId, UpdateTransactionHeaderDto updateDto)
        {
            var transactionHeader = await _transactionRepository.GetTransactionByIdAsync(transactionHeaderId);
            if (transactionHeader == null)
            {
                // Or throw a custom not found exception
                return;
            }

            _mapper.Map(updateDto, transactionHeader);
            await _transactionRepository.UpdateTransactionHeaderAsync(transactionHeader);
        }
    }
}
