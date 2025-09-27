using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Repository.Core.Interface;

namespace BusinessLogic.Core.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly IBusinessRepository _businessRepository;
        private readonly IMapper _mapper;

        public BusinessService(IBusinessRepository businessRepository, IMapper mapper)
        {
            _businessRepository = businessRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BusinessDto>> GetAllBusinessesAsync()
        {
            var businesses = await _businessRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BusinessDto>>(businesses);
        }

        public async Task<BusinessDto?> GetBusinessByIdAsync(Guid id)
        {
            var business = await _businessRepository.GetByIdAsync(id);
            return _mapper.Map<BusinessDto>(business);
        }

        public async Task<BusinessDto> CreateBusinessAsync(BusinessCreateDto businessCreateDto, Guid creatingUserId)
        {
            var business = _mapper.Map<Business>(businessCreateDto);

            business.BusinessID = Guid.NewGuid();
            business.IsActive = true;
            business.CreatedByUserID = creatingUserId;
            business.UpdatedByUserID = creatingUserId;
            business.CreatedAtUTC = DateTime.UtcNow;
            business.UpdatedAtUTC = DateTime.UtcNow;

            var addedBusiness = await _businessRepository.AddAsync(business);
            return _mapper.Map<BusinessDto>(addedBusiness);
        }

        public async Task<bool> UpdateBusinessAsync(Guid id, BusinessUpdateDto businessUpdateDto, Guid updatingUserId)
        {
            var business = await _businessRepository.GetByIdAsync(id);
            if (business == null)
            {
                return false; // Business not found
            }

            _mapper.Map(businessUpdateDto, business);
            business.UpdatedByUserID = updatingUserId;
            business.UpdatedAtUTC = DateTime.UtcNow;

            await _businessRepository.UpdateAsync(business);
            return true;
        }

        public async Task<bool> DeleteBusinessAsync(Guid id)
        {
            return await _businessRepository.DeleteAsync(id);
        }
    }
}