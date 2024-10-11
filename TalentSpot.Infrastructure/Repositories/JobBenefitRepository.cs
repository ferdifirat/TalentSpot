using TalentSpot.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using TalentSpot.Infrastructure.Data;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Infrastructure.Repositories
{
    public class JobBenefitRepository : BaseRepository<JobBenefit>, IJobBenefitRepository
    {
        public JobBenefitRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<JobBenefit>> GetBenefitsByJobIdAsync(Guid jobId)
        {
            return await _context.JobBenefits.Where(p=>p.JobId == jobId).ToListAsync();
        }
    }
}
