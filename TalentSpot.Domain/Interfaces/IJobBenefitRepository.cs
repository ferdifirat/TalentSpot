using TalentSpot.Domain.Entities;

namespace TalentSpot.Domain.Interfaces
{
    public interface IJobBenefitRepository : IBaseRepository<JobBenefit>
    {
        Task<List<JobBenefit>> GetBenefitsByJobIdAsync(Guid jobId);
    }
}
