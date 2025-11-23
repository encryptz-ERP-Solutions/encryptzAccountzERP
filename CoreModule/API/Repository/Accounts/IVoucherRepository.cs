using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Entities.Accounts;

namespace Repository.Accounts
{
    public interface IVoucherRepository
    {
        Task<Voucher> GetByIdAsync(Guid voucherId);
        Task<IEnumerable<Voucher>> GetAllByBusinessIdAsync(Guid businessId, string? status = null, string? voucherType = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<Voucher> CreateAsync(Voucher voucher);
        Task<bool> UpdateAsync(Voucher voucher);
        Task<bool> DeleteAsync(Guid voucherId);
        Task<string> GenerateVoucherNumberAsync(Guid businessId, string voucherType);
        Task<bool> PostVoucherAsync(Guid voucherId, Guid postedBy);
        Task<IEnumerable<VoucherLine>> GetVoucherLinesAsync(Guid voucherId);
    }
}
