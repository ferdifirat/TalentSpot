using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Application.Services
{
    public interface ICompanyService
    {
        Task<CompanyDTO> RegisterCompanyAsync(CompanyDTO companyDTO);
        Task<CompanyDTO> GetCompanyAsync(Guid id);
        Task<bool> UpdateCompanyAsync(CompanyDTO companyDTO);
    }
}