namespace TalentSpot.Application.DTOs
{
    public class JobCreateDTO
    {
        public string Position { get; set; }
        public string Description { get; set; }
        public string? Benefits { get; set; }
        public string WorkType { get; set; }
        public decimal? Salary { get; set; }
        public Guid CompanyId { get; set; }
    }
}
