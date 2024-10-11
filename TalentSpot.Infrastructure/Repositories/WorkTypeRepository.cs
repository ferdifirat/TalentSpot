using TalentSpot.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using TalentSpot.Infrastructure.Data;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Infrastructure.Repositories
{
    public class WorkTypeRepository : BaseRepository<WorkType>, IWorkTypeRepository
    {
        public WorkTypeRepository(ApplicationDbContext context) : base(context) { }
    }
}
