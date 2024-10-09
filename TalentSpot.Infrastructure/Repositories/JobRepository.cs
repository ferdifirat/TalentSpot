using Microsoft.EntityFrameworkCore;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;
using TalentSpot.Infrastructure.Data;

namespace TalentSpot.Infrastructure.Repositories
{
    public class JobRepository : BaseRepository<Job>, IJobRepository

    {
        public JobRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Job>> GetJobsByExpirationDateAsync(DateTime expirationDate)
        {
            return await _context.Jobs
                .Where(j => j.ExpirationDate >= expirationDate)
                .ToListAsync();
        }
    }
}
