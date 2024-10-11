using TalentSpot.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using TalentSpot.Infrastructure.Data;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<User> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _context.Users.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        }
    }
}
