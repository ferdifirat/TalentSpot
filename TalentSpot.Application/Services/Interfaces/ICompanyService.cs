using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Application.Services
{
    public interface ICompanyService
    {
        Task<ResponseMessage<CompanyDTO>> GetCompanyAsync(Guid id);
        Task<ResponseMessage<CompanyDTO>> UpdateCompanyAsync(CompanyDTO companyDTO);
    }
}