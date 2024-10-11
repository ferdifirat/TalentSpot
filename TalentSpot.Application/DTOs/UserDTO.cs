namespace TalentSpot.Application.DTOs
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public CompanyDTO Company { get; set; }
    }
}
