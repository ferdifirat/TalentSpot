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
        public DateTime ExpirationDate { get; set; } // Yayında kalma süresi

        public int QualityScore { get; set; }

        public string Benefits { get; set; }

        public string WorkType { get; set; }

        public decimal? Salary { get; set; }

        public Guid CompanyId { get; set; } // Foreign Key
        public Company Company { get; set; } // Navigation Property
    }
}
