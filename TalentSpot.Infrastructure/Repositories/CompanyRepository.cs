using TalentSpot.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using TalentSpot.Infrastructure.Data;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Infrastructure.Repositories
{
    public class CompanyRepository : BaseRepository<Company>, ICompanyRepository
    {
        public CompanyRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Company> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Companies.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        }
    }
}
