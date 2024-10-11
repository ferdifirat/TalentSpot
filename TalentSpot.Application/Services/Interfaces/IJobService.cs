using TalentSpot.Application.DTOs;

namespace TalentSpot.Application.Services
{
    public interface IJobService
    {
        Task<ResponseMessage<JobDTO>> CreateJobAsync(JobCreateDTO jobDTO);

        Task<ResponseMessage<JobDTO>> GetJobAsync(Guid id);

        Task<ResponseMessage<List<JobDTO>>> GetAllJobAsync();

        Task<bool> UpdateJobAsync(JobUpdateDTO jobUpdateDTO);

        Task<bool> DeleteJobAsync(Guid id);

        Task<ResponseMessage<List<JobDTO>>> SearchJobsByExpirationDateAsync(DateTime expirationDate);
    }
}