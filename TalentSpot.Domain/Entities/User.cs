using System.ComponentModel.DataAnnotations;

namespace TalentSpot.Domain.Entities
{
    public class User : BaseEntity
    {
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        [Required]
        public string Email { get; set; }
        public Company Company { get; set; }
    }
}
