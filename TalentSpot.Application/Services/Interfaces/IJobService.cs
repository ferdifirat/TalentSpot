using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Interfaces;

namespace TalentSpot.Application.Services
{
    public interface IJobService
    {
        Task<JobDTO> CreateJobAsync(JobDTO jobDTO);
        Task<JobDTO> GetJobAsync(Guid id);
        Task<bool> UpdateJobAsync(JobDTO jobDTO);
        Task<bool> DeleteJobAsync(Guid id);
        Task<List<JobDTO>> GetAllJobAsync();
        Task<IEnumerable<JobDTO>> SearchJobsByExpirationDateAsync(DateTime expirationDate);
    }
}