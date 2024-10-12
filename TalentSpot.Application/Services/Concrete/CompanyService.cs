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

        public async Task<ResponseMessage<List<CompanyDTO>>> GetAllCompanyAsync()
        {

            var includes = new List<string> { $"{nameof(Company.User)}", };
            var companies = (await _companyRepository.List<Company>(true, includes)).ToList();

            if (companies == null || !companies.Any())
            {
                return ResponseMessage<List<CompanyDTO>>.FailureResponse("İlan bulunamadı.");
            }

            var companyDTOs = new List<CompanyDTO>();

            foreach (var company in companies)
            {
                companyDTOs.Add(new CompanyDTO
                {
                    Id = company.Id,
                    Name = company.Name,
                    Address = company.Address,
                    AllowedJobPostings = company.AllowedJobPostings,
                    User = new UserDTO()
                    {
                        Id = company.User.Id,
                        PhoneNumber = company.User.PhoneNumber
                    }
                });
            }

            return ResponseMessage<List<CompanyDTO>>.SuccessResponse(companyDTOs);
        }

        public async Task<ResponseMessage<CompanyDTO>> GetCompanyAsync(Guid id)
        {
            var company = await _companyRepository.GetById<Company>(id, true);
            if (company == null)
            {
                return ResponseMessage<CompanyDTO>.FailureResponse("Şirket bulunamadı.");
            }

            return ResponseMessage<CompanyDTO>.SuccessResponse(new CompanyDTO
            {
                Id = company.Id,
                Name = company.Name,
                Address = company.Address,
                AllowedJobPostings = company.AllowedJobPostings,
                User = new UserDTO()
                {
                    PhoneNumber = company.User.PhoneNumber,
                    Id = company.User.Id,
                    Email = company.User.Email,
                }

            });
        }

        public async Task<ResponseMessage<CompanyDTO>> UpdateCompanyAsync(CompanyDTO companyDTO)
        {
            var existingCompany = await _companyRepository.GetById<Company>(companyDTO.Id, true);
            if (existingCompany == null)
            {
                return ResponseMessage<CompanyDTO>.FailureResponse("Şirket bulunamadı.");
            }

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
                Name = existingCompany.Name,
                Address = existingCompany.Address,
                AllowedJobPostings = existingCompany.AllowedJobPostings
            });
        }
    }
}
