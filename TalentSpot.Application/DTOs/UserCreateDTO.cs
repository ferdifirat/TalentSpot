namespace TalentSpot.Application.DTOs
{
    public class UserCreateDTO
    {
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
    }
}
