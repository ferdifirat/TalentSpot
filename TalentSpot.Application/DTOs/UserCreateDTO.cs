namespace TalentSpot.Application.DTOs
{
    public class UserCreateDTO
    {
        public string PhoneNumber { get; set; } // Zorunlu
        public string Password { get; set; } // Zorunlu
        public string Email { get; set; } // Zorunlu
        public string CompanyName { get; set; } // Zorunlu
        public string Address { get; set; } // Zorunlu
    }
}
