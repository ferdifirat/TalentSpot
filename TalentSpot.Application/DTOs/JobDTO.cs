using TalentSpot.Domain.Entities;

namespace TalentSpot.Application.DTOs
{
    public class JobDTO
    {
        public Guid Id { get; set; }
        public string Position { get; set; }
        public string Description { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int QualityScore { get; set; }
        public List<Benefit> Benefits { get; set; }
        public WorkType WorkType{ get; set; }
        public decimal? Salary { get; set; }
        public Guid CompanyId { get; set; }
    }
}
