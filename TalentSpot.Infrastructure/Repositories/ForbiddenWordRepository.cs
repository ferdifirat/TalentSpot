using TalentSpot.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using TalentSpot.Infrastructure.Data;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Infrastructure.Repositories
{
    public class ForbiddenWordRepository : BaseRepository<ForbiddenWord>, IForbiddenWordRepository
    {
        public ForbiddenWordRepository(ApplicationDbContext context) : base(context) { }
    }
}
