using System.ComponentModel.DataAnnotations;

namespace TalentSpot.Domain.Entities
{
    public class Job : BaseEntity
    {
        [Required]
        public string Position { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        public int QualityScore { get; set; }

        public ICollection<JobBenefit> JobBenefits { get; set; }
        public Guid? WorkTypeId { get; set; }
        public WorkType WorkType { get; set; }

        public decimal? Salary { get; set; }

        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
