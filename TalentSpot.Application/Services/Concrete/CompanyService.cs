using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;

namespace TalentSpot.Application.Services.Concrete
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CompanyService(ICompanyRepository companyRepository, IUnitOfWork unitOfWork)
        {
            _companyRepository = companyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMessage<CompanyDTO>> RegisterCompanyAsync(CompanyCreateDTO companyDTO)
        {
            var existingCompany = await _companyRepository.GetByPhoneNumberAsync(companyDTO.PhoneNumber);
            if (existingCompany != null)
            {
                return ResponseMessage<CompanyDTO>.FailureResponse("Bu telefon numarasıyla kayıtlı bir firma bulunmaktadır.");
            }

            var company = new Company
            {
                Id = Guid.NewGuid(),
                PhoneNumber = companyDTO.PhoneNumber,
                Name = companyDTO.Name,
                Address = companyDTO.Address
            };

            await _companyRepository.AddAsync(company);
            var isCreated = await _unitOfWork.CompleteAsync();

            if (!isCreated)
            {
                return ResponseMessage<CompanyDTO>.FailureResponse("Şirket bilgileri kaydedilemedi.");
            }

            return ResponseMessage<CompanyDTO>.SuccessResponse(new CompanyDTO
            {
                Id = company.Id,
                PhoneNumber = company.PhoneNumber,
                Name = company.Name,
                Address = company.Address,
                AllowedJobPostings = company.AllowedJobPostings
            });
        }

        public async Task<ResponseMessage<CompanyDTO>> GetCompanyAsync(Guid id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
            {
                return ResponseMessage<CompanyDTO>.FailureResponse("Şirket bulunamadı.");
            }

            return ResponseMessage<CompanyDTO>.SuccessResponse(new CompanyDTO
            {
                Id = company.Id,
                PhoneNumber = company.PhoneNumber,
                Name = company.Name,
                Address = company.Address,
                AllowedJobPostings = company.AllowedJobPostings
            });
        }

        public async Task<ResponseMessage<CompanyDTO>> UpdateCompanyAsync(CompanyDTO companyDTO)
        {
            var existingCompany = await _companyRepository.GetByIdAsync(companyDTO.Id);
            if (existingCompany == null)
            {
                return ResponseMessage<CompanyDTO>.FailureResponse("Şirket bulunamadı.");
            }

            existingCompany.PhoneNumber = companyDTO.PhoneNumber;
            existingCompany.Name = companyDTO.Name;
            existingCompany.Address = companyDTO.Address;

            await _companyRepository.UpdateAsync(existingCompany);

            var isUpdated = await _unitOfWork.CompleteAsync();

            if (!isUpdated)
            {
                return ResponseMessage<CompanyDTO>.FailureResponse("Şirket bilgileri güncellenemedi.");
            }

            return ResponseMessage<CompanyDTO>.SuccessResponse(new CompanyDTO()
            {
                Id = existingCompany.Id,
                PhoneNumber = existingCompany.PhoneNumber,
                Name = existingCompany.Name,
                Address = existingCompany.Address,
                AllowedJobPostings = existingCompany.AllowedJobPostings
            });
        }
    }
}
