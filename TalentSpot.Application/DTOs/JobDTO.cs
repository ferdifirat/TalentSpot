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
        public List<BenefitDTO> Benefits { get; set; }
        public WorkTypeDTO WorkType{ get; set; }
        public Guid WorkTypeId{ get; set; }
        public decimal? Salary { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
