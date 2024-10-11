using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;

namespace TalentSpot.Application.Services
{
    public interface IWorkTypeService
    {
        Task<IEnumerable<WorkType>> GetAllWorkTypesAsync();
        Task<WorkType> GetWorkTypeByIdAsync(Guid id);
        Task AddWorkTypeAsync(WorkType workType);
        Task UpdateWorkTypeAsync(WorkType workType);
        Task DeleteWorkTypeAsync(Guid id);
    }
}