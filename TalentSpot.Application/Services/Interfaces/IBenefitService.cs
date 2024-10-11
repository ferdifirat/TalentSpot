using TalentSpot.Domain.Entities;

namespace TalentSpot.Application.Services
{
    public interface IBenefitService
    {
        Task<IEnumerable<Benefit>> GetAllBenefitsAsync();
        Task<Benefit> GetBenefitByIdAsync(Guid id);
        Task AddBenefitAsync(Benefit benefit);
        Task UpdateBenefitAsync(Benefit benefit);
        Task DeleteBenefitAsync(Guid id);
    }
}