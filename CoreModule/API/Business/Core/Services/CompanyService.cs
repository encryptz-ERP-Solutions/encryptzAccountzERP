using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;
using AutoMapper;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Repository.Core.Interface;

namespace BusinessLogic.Core.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;

        public CompanyService(ICompanyRepository companyRepository, IMapper mapper)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
        }

        public async Task<bool> AddCompanyAsync(CompanyDto companyDto)
        {
            var company = _mapper.Map<Company>(companyDto);
            await _companyRepository.AddAsync(company);
            return true;
        }

        public async Task<bool> DeleteCompanyAsync(long id)
        {
            await _companyRepository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<CompanyDto?>> GetAllCompanyAsync()
        {
            var companies = await _companyRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<CompanyDto>>(companies);
        }

        public async Task<CompanyDto?> GetCompanyByIdAsync(long id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            return _mapper.Map<CompanyDto>(company);
        }

        public async Task<bool> UpdateCompanyAsync(CompanyDto companyDto)
        {
            var company = _mapper.Map<Company>(companyDto);
            await _companyRepository.UpdateAsync(company);
            return true;
        }
    }
}
