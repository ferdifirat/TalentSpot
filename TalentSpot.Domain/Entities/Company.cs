using System.ComponentModel.DataAnnotations;

namespace TalentSpot.Domain.Entities
{
    public class Company : BaseEntity
    {
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public int AllowedJobPostings { get; set; } = 2;
    }
}
