using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Interfaces;

namespace TalentSpot.Application.Services
{
    public interface IJobService
    {
        Task<ResponseMessage<JobDTO>> CreateJobAsync(JobCreateDTO jobDTO);
        Task<ResponseMessage<JobDTO>> GetJobAsync(Guid id);
        Task<bool> UpdateJobAsync(JobDTO jobDTO);
        Task<bool> DeleteJobAsync(Guid id);
        Task<ResponseMessage<List<JobDTO>>> GetAllJobAsync();
        Task<ResponseMessage<List<JobDTO>>> SearchJobsByExpirationDateAsync(DateTime expirationDate);
    }
}