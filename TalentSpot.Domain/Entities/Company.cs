using System.ComponentModel.DataAnnotations;

namespace TalentSpot.Domain.Entities
{
    public class Company : BaseEntity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int AllowedJobPostings { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public List<Job> Jobs { get; set; }
    }
}
