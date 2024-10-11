using TalentSpot.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using TalentSpot.Infrastructure.Data;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Infrastructure.Repositories
{
    public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Company> GetCompanyByUserId(Guid userId)
        {
            return await _context.Companies.FirstOrDefaultAsync(p => p.UserId == userId);
        }
    }
}
