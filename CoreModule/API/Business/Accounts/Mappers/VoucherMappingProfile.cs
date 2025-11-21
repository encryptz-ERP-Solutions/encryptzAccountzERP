using AutoMapper;
using BusinessLogic.Accounts.DTOs;
using Entities.Accounts;

namespace BusinessLogic.Accounts.Mappers
{
    public class VoucherMappingProfile : Profile
    {
        public VoucherMappingProfile()
        {
            // Voucher mappings
            CreateMap<Voucher, VoucherDto>()
                .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.VoucherLines));

            CreateMap<CreateVoucherDto, Voucher>()
                .ForMember(dest => dest.VoucherID, opt => opt.Ignore())
                .ForMember(dest => dest.VoucherNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.PostedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PostedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TaxAmount, opt => opt.Ignore())
                .ForMember(dest => dest.DiscountAmount, opt => opt.Ignore())
                .ForMember(dest => dest.RoundOffAmount, opt => opt.Ignore())
                .ForMember(dest => dest.NetAmount, opt => opt.Ignore())
                .ForMember(dest => dest.VoucherLines, opt => opt.Ignore());

            // VoucherLine mappings
            CreateMap<VoucherLine, VoucherLineDto>();

            CreateMap<CreateVoucherLineDto, VoucherLine>()
                .ForMember(dest => dest.LineID, opt => opt.Ignore())
                .ForMember(dest => dest.VoucherID, opt => opt.Ignore())
                .ForMember(dest => dest.LineNumber, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}
