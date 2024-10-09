using TalentSpot.Domain.Entities;

namespace TalentSpot.Domain.Interfaces
{
    public interface IJobRepository : IBaseRepository<Job>
    {
        Task<IEnumerable<Job>> GetJobsByExpirationDateAsync(DateTime expirationDate);
    }
}
