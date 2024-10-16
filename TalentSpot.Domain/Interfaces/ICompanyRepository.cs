﻿using TalentSpot.Domain.Entities;

namespace TalentSpot.Domain.Interfaces
{
    public interface ICompanyRepository : IBaseRepository<Company>
    {
        Task<Company> GetCompanyByUserId(Guid userId);
    }
}
