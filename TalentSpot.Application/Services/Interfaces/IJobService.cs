using TalentSpot.Application.DTOs;

namespace TalentSpot.Application.Services
{
    public interface IJobService
    {
        Task<ResponseMessage<JobDTO>> CreateJobAsync(JobCreateDTO jobDTO, Guid userId);

        Task<ResponseMessage<JobDTO>> GetJobAsync(Guid id);

        Task<ResponseMessage<List<JobDTO>>> GetAllJobAsync();

        Task<ResponseMessage<JobDTO>> UpdateJobAsync(JobUpdateDTO jobUpdateDTO);

        Task<bool> DeleteJobAsync(Guid id);

        Task<ResponseMessage<List<JobDTO>>> SearchJobsByExpirationDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}