using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.Core.DTOs;
using BusinessLogic.Core.Interface;
using Entities.Core;
using Infrastructure.Extensions;
using Repository.Admin.Interface;
using Repository.Core.Interface;

namespace BusinessLogic.Core.Services
{
    public class CompanyService:ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        public CompanyService(ICompanyRepository companyRepository) 
        {
            _companyRepository = companyRepository;
        }

        public async Task<bool> AddCompanyAsync(CompanyDto Company)
        {
            try
            {
                await _companyRepository.AddAsync(Company.ConvertToClassObject<CompanyDto, Company>());
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> DeleteCompanyAsync(long id)
        {
            try
            {
                await _companyRepository.DeleteAsync(id);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<CompanyDto?>> GetAllCompanyAsync()
        {
            try
            {
                var companies = await _companyRepository.GetAllAsync();
                var companyDtoList = new List<CompanyDto>();
                foreach (var company in companies)
                {
                    var companyDto = new CompanyDto();
                    companyDto = company.ConvertToClassObject<Company, CompanyDto>();
                    companyDtoList.Add(companyDto);
                }
                return companyDtoList;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<CompanyDto?> GetCompanyByIdAsync(long id)
        {
            try
            {

                var company = await _companyRepository.GetByIdAsync(id);

                var companyDto = new CompanyDto();
                companyDto=company.ConvertToClassObject<Company, CompanyDto>();

                return companyDto;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> UpdateCompanyAsync(CompanyDto Company)
        {
            try
            {
                await _companyRepository.UpdateAsync(Company.ConvertToClassObject<CompanyDto, Company>());

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
