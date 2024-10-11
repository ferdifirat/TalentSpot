using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Application.Services
{
    public interface IBenefitService
    {
        Task<IEnumerable<Benefit>> GetAllBenefitsAsync();
        Task<Benefit> GetBenefitByIdAsync(Guid id);
        Task<ResponseMessage<Benefit>> AddBenefitAsync(Benefit benefit);
        Task<ResponseMessage<Benefit>> UpdateBenefitAsync(Benefit benefit);
        Task DeleteBenefitAsync(Guid id);
    }
}