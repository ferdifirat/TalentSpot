using TalentSpot.Application.DTOs;

namespace TalentSpot.Application.Services
{
    public interface ICompanyService
    {
        Task<ResponseMessage<List<CompanyDTO>>> GetAllCompanyAsync();
        Task<ResponseMessage<CompanyDTO>> GetCompanyAsync(Guid id);
        Task<ResponseMessage<CompanyDTO>> UpdateCompanyAsync(CompanyDTO companyDTO);
    }
}