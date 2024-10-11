using System.ComponentModel.DataAnnotations;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Application.DTOs
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
    }
}
