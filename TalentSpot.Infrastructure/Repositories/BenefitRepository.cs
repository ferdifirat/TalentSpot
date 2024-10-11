using TalentSpot.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using TalentSpot.Infrastructure.Data;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Infrastructure.Repositories
{
    public class BenefitRepository : BaseRepository<Benefit>, IBenefitRepository
    {
        public BenefitRepository(ApplicationDbContext context) : base(context) { }
    }
}
