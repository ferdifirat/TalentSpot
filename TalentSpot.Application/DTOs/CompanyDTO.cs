namespace TalentSpot.Application.DTOs
{
    public class CompanyDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int? AllowedJobPostings { get; set; }
        public UserDTO User { get; set; }   
    }
}
