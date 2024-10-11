using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;

namespace TalentSpot.Application.Services.Concrete
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IForbiddenWordsService _forbiddenWordsService;
        private readonly IUnitOfWork _unitOfWork;

        public JobService(IJobRepository jobRepository,
            ICompanyRepository companyRepository,
            IForbiddenWordsService forbiddenWordsService,
            IUnitOfWork unitOfWork)
        {
            _jobRepository = jobRepository;
            _companyRepository = companyRepository;
            _forbiddenWordsService = forbiddenWordsService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseMessage<JobDTO>> CreateJobAsync(JobCreateDTO jobDTO)
        {
            var company = await _companyRepository.GetByIdAsync(jobDTO.CompanyId);
            if (company == null)
            {
                ResponseMessage<JobDTO>.FailureResponse("Şirket bulunamadı.");
            }

            if (company?.AllowedJobPostings == 0)
            {
                ResponseMessage<JobDTO>.FailureResponse("Bu şirketin ilan yayınlama hakkı kalmamıştır.");
            }

            var job = new Job
            {
                Id = Guid.NewGuid(),
                Position = jobDTO.Position,
                Description = jobDTO.Description,
                ExpirationDate = DateTime.UtcNow.AddDays(15),
                QualityScore = await CalculateQualityScoreAsync(jobDTO),
                Benefits = jobDTO.Benefits,
                WorkType = jobDTO.WorkType,
                Salary = jobDTO.Salary,
                CompanyId = jobDTO.CompanyId
            };

            try
            {
                await _unitOfWork.BeginTransactionAsync();
                await _jobRepository.AddAsync(job);
                await _unitOfWork.CompleteAsync();

                company.AllowedJobPostings--;
                await _companyRepository.UpdateAsync(company);
                await _unitOfWork.CompleteAsync();

                await _unitOfWork.CommitAsync();
                return ResponseMessage<JobDTO>.SuccessResponse(new JobDTO
                {
                    Id = job.Id,
                    Position = job.Position,
                    Description = job.Description,
                    ExpirationDate = job.ExpirationDate,
                    QualityScore = job.QualityScore,
                    Benefits = job.Benefits,
                    WorkType = job.WorkType,
                    Salary = job.Salary,
                    CompanyId = job.CompanyId
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ResponseMessage<JobDTO>.FailureResponse("Bir hata oluştu: " + ex.Message);
            }
        }

        public async Task<ResponseMessage<JobDTO>> GetJobAsync(Guid id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                ResponseMessage<JobDTO>.FailureResponse("İlan bulunamadı.");
            }

            return ResponseMessage<JobDTO>.SuccessResponse(new JobDTO
            {
                Id = job.Id,
                Position = job.Position,
                Description = job.Description,
                ExpirationDate = job.ExpirationDate,
                QualityScore = job.QualityScore,
                Benefits = job.Benefits,
                WorkType = job.WorkType,
                Salary = job.Salary,
                CompanyId = job.CompanyId
            });
        }

        public async Task<ResponseMessage<List<JobDTO>>> GetAllJobAsync()
        {
            var jobs = await _jobRepository.GetAllAsync();
            if (jobs == null)
            {
                ResponseMessage<List<JobDTO>>.FailureResponse("İlan bulunamadı.");
            }

            var jobDTOs = jobs.Select(p => new JobDTO
            {
                Id = p.Id,
                Position = p.Position,
                Description = p.Description,
                ExpirationDate = p.ExpirationDate,
                QualityScore = p.QualityScore,
                Benefits = p.Benefits,
                WorkType = p.WorkType,
                Salary = p.Salary,
                CompanyId = p.CompanyId
            }).ToList();

            return ResponseMessage<List<JobDTO>>.SuccessResponse(jobDTOs);
        }

        public async Task<bool> UpdateJobAsync(JobDTO jobDTO)
        {
            var existingJob = await _jobRepository.GetByIdAsync(jobDTO.Id);
            if (existingJob == null)
            {
                ResponseMessage<List<JobDTO>>.FailureResponse("İlan bulunamadı.");
            }

            // Update the job properties
            existingJob.Position = jobDTO.Position;
            existingJob.Description = jobDTO.Description;
            existingJob.ExpirationDate = jobDTO.ExpirationDate;
            existingJob.Benefits = jobDTO.Benefits;
            existingJob.WorkType = jobDTO.WorkType;
            existingJob.Salary = jobDTO.Salary;

            await _jobRepository.UpdateAsync(existingJob);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteJobAsync(Guid id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                ResponseMessage<List<JobDTO>>.FailureResponse("İlan bulunamadı.");
            }

            await _jobRepository.DeleteAsync(id);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<ResponseMessage<List<JobDTO>>> SearchJobsByExpirationDateAsync(DateTime expirationDate)
        {
            var jobs = await _jobRepository.FindAsync(j => j.ExpirationDate.Date == expirationDate.Date);

            var jobDTOs = new List<JobDTO>();
            foreach (var job in jobs)
            {
                jobDTOs.Add(new JobDTO
                {
                    Id = job.Id,
                    Position = job.Position,
                    Description = job.Description,
                    ExpirationDate = job.ExpirationDate,
                    QualityScore = job.QualityScore,
                    Benefits = job.Benefits,
                    WorkType = job.WorkType,
                    Salary = job.Salary,
                    CompanyId = job.CompanyId
                });
            }

            return ResponseMessage<List<JobDTO>>.SuccessResponse(jobDTOs);
        }

        private async Task<bool> ContainsForbiddenWordsAsync(string description)
        {
            var forbiddenWords = await _forbiddenWordsService.GetForbiddenWordsAsync();
            return forbiddenWords.Any(word => description.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<int> CalculateQualityScoreAsync(JobCreateDTO jobDTO)
        {
            int score = 0;

            if (!string.IsNullOrEmpty(jobDTO.WorkType))
            {
                score++;
            }

            if (jobDTO.Salary.HasValue)
            {
                score++;
            }

            if (!string.IsNullOrEmpty(jobDTO.Benefits))
            {
                score++;
            }

            if (!await ContainsForbiddenWordsAsync(jobDTO.Description))
            {
                score += 2;
            }

            return score;
        }
    }
}