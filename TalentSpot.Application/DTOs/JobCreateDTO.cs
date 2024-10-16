﻿namespace TalentSpot.Application.DTOs
{
    public class JobCreateDTO
    {
        public string Position { get; set; }
        public string Description { get; set; }
        public List<Guid>? BenefitIds { get; set; }
        public Guid? WorkTypeId { get; set; }
        public int? Salary { get; set; }
    }
}
