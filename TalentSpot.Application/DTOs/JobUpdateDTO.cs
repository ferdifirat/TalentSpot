using TalentSpot.Domain.Entities;

namespace TalentSpot.Application.DTOs
{
    public class JobUpdateDTO
    {
        public Guid Id { get; set; }
        public string Position { get; set; }
        public string Description { get; set; }
        public List<Guid> BenefitIds { get; set; }
        public Guid WorkTypeId { get; set; }
        public decimal? Salary { get; set; }
    }
}
