using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;

namespace TalentSpot.Application.Services.Concrete
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<CompanyDTO> RegisterCompanyAsync(CompanyDTO companyDTO)
        {
            // Phone number validation
            var existingCompany = await _companyRepository.GetByPhoneNumberAsync(companyDTO.PhoneNumber);
            if (existingCompany != null)
            {
                throw new Exception("Bu telefon numarasıyla kayıtlı bir firma bulunmaktadır.");
            }

            // Create a new Company entity from DTO
            var company = new Company
            {
                Id = Guid.NewGuid(), // Assuming you want to use GUID for Id
                PhoneNumber = companyDTO.PhoneNumber,
                Name = companyDTO.Name,
                Address = companyDTO.Address,
                AllowedJobPostings = companyDTO.AllowedJobPostings // Set the allowed postings from DTO
            };

            await _companyRepository.AddAsync(company);
            await _companyRepository.SaveChangesAsync();
            // Return the DTO
            return new CompanyDTO
            {
                Id = company.Id,
                PhoneNumber = company.PhoneNumber,
                Name = company.Name,
                Address = company.Address,
                AllowedJobPostings = company.AllowedJobPostings
            };
        }

        public async Task<CompanyDTO> GetCompanyAsync(Guid id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                throw new Exception("Şirket bulunamadı.");
            }

            // Convert the Company entity to DTO
            return new CompanyDTO
            {
                Id = company.Id,
                PhoneNumber = company.PhoneNumber,
                Name = company.Name,
                Address = company.Address,
                AllowedJobPostings = company.AllowedJobPostings
            };
        }

        public async Task<bool> UpdateCompanyAsync(CompanyDTO companyDTO)
        {
            var existingCompany = await _companyRepository.GetByIdAsync(companyDTO.Id);
            if (existingCompany == null)
            {
                throw new Exception("Şirket bulunamadı.");
            }

            // Update company properties
            existingCompany.PhoneNumber = companyDTO.PhoneNumber;
            existingCompany.Name = companyDTO.Name;
            existingCompany.Address = companyDTO.Address;
            // Update other properties as needed

            await _companyRepository.UpdateAsync(existingCompany);

            return await _companyRepository.SaveChangesAsync();
        }
    }
}
