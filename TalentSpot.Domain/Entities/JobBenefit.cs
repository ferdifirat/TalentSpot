namespace TalentSpot.Domain.Entities
{
    public class JobBenefit : BaseEntity
    {
        public Job Job { get; set; }

        public Guid BenefitId { get; set; }
        public Guid JobId { get; set; }
        public Benefit Benefit { get; set; }
    }
}
