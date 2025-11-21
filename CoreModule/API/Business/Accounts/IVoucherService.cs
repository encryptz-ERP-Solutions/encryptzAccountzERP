using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessLogic.Accounts.DTOs;

namespace BusinessLogic.Accounts
{
    public interface IVoucherService
    {
        Task<VoucherDto> GetVoucherByIdAsync(Guid voucherId);
        Task<IEnumerable<VoucherDto>> GetVouchersByBusinessIdAsync(Guid businessId, string? status = null, string? voucherType = null, DateTime? startDate = null, DateTime? endDate = null);
        Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto createDto, Guid createdBy);
        Task<VoucherDto> UpdateVoucherAsync(Guid voucherId, UpdateVoucherDto updateDto, Guid updatedBy);
        Task<bool> DeleteVoucherAsync(Guid voucherId);
        Task<PostVoucherResponseDto> PostVoucherAsync(Guid voucherId, Guid postedBy);
    }
}
