using AutoMapper;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Repository.Core.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<BusinessDto> GetBusinessByIdAsync(Guid id)
        {
            var business = await _businessRepository.GetByIdAsync(id);
            return _mapper.Map<BusinessDto>(business);
        }

        public async Task<BusinessDto> AddBusinessAsync(BusinessDto businessDto)
        {
            var business = _mapper.Map<Business>(businessDto);
            // You might need to set CreatedByUserID and UpdatedByUserID from the current user context
            business.CreatedByUserID = Guid.NewGuid(); // Placeholder
            business.UpdatedByUserID = Guid.NewGuid(); // Placeholder
            business.BusinessID = Guid.NewGuid();
            business.BusinessCode = "TEMP"; // Placeholder
            var newBusiness = await _businessRepository.AddAsync(business);
            return _mapper.Map<BusinessDto>(newBusiness);
        }

        public async Task<bool> UpdateBusinessAsync(Guid id, BusinessDto businessDto)
        {
            var business = await _businessRepository.GetByIdAsync(id);
            if (business == null)
            {
                return false;
            }

            _mapper.Map(businessDto, business);
            business.BusinessID = id; // Ensure the ID is not changed
            // You might need to set UpdatedByUserID from the current user context
            business.UpdatedByUserID = Guid.NewGuid(); // Placeholder
            return await _businessRepository.UpdateAsync(business);
        }

        public async Task<bool> DeleteBusinessAsync(Guid id)
        {
            return await _businessRepository.DeleteAsync(id);
        }
    }
}