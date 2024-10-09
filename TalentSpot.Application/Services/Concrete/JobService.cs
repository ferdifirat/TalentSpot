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

        public JobService(IJobRepository jobRepository, ICompanyRepository companyRepository, IForbiddenWordsService forbiddenWordsService)
        {
            _jobRepository = jobRepository;
            _companyRepository = companyRepository;
            _forbiddenWordsService = forbiddenWordsService;
        }

        public async Task<JobDTO> CreateJobAsync(JobDTO jobDTO)
        {
            // Validate that the company exists
            var company = await _companyRepository.GetByIdAsync(jobDTO.CompanyId);
            if (company == null)
            {
                throw new Exception("Şirket bulunamadı.");
            }

            // Check if the company has job postings left
            if (company.AllowedJobPostings <= 0)
            {
                throw new Exception("Bu şirketin ilan yayınlama hakkı kalmamıştır.");
            }

            // Create the Job entity
            var job = new Job
            {
                Id = Guid.NewGuid(), // Use GUID for the job ID
                Position = jobDTO.Position,
                Description = jobDTO.Description,
                ExpirationDate = DateTime.UtcNow.AddDays(15), // Set expiration date to 15 days from now
                QualityScore = await CalculateQualityScoreAsync(jobDTO), // Calculate quality score
                Benefits = jobDTO.Benefits,
                WorkType = jobDTO.WorkType,
                Salary = jobDTO.Salary,
                CompanyId = jobDTO.CompanyId
            };

            // Save the job entity
            await _jobRepository.AddAsync(job);
            await _jobRepository.SaveChangesAsync();

            // Decrease the company's allowed job postings
            company.AllowedJobPostings--;
            await _companyRepository.UpdateAsync(company);
            await _companyRepository.SaveChangesAsync();

            // Return the created JobDTO
            return new JobDTO
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
            };
        }

        public async Task<JobDTO> GetJobAsync(Guid id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                throw new Exception("İlan bulunamadı.");
            }

            return new JobDTO
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
            };
        }

        public async Task<List<JobDTO>> GetAllJobAsync()
        {
            var jobs = await _jobRepository.GetAllAsync();
            if (jobs == null)
            {
                throw new Exception("İlan bulunamadı.");
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

            return jobDTOs;
        }

        public async Task<bool> UpdateJobAsync(JobDTO jobDTO)
        {
            var existingJob = await _jobRepository.GetByIdAsync(jobDTO.Id);
            if (existingJob == null)
            {
                throw new Exception("İlan bulunamadı.");
            }

            // Update the job properties
            existingJob.Position = jobDTO.Position;
            existingJob.Description = jobDTO.Description;
            existingJob.ExpirationDate = jobDTO.ExpirationDate;
            existingJob.Benefits = jobDTO.Benefits;
            existingJob.WorkType = jobDTO.WorkType;
            existingJob.Salary = jobDTO.Salary;

            await _jobRepository.UpdateAsync(existingJob);
            return await _jobRepository.SaveChangesAsync();
        }

        public async Task<bool> DeleteJobAsync(Guid id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                throw new Exception("İlan bulunamadı.");
            }

            await _jobRepository.DeleteAsync(id);
            return await _jobRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<JobDTO>> SearchJobsByExpirationDateAsync(DateTime expirationDate)
        {
            // Get jobs with the specified expiration date
            var jobs = await _jobRepository.FindAsync(j => j.ExpirationDate.Date == expirationDate.Date);

            // Map the jobs to JobDTO
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

            return jobDTOs;
        }

        private async Task<bool> ContainsForbiddenWordsAsync(string description)
        {
            var forbiddenWords = await _forbiddenWordsService.GetForbiddenWordsAsync();
            return forbiddenWords.Any(word => description.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<int> CalculateQualityScoreAsync(JobDTO jobDTO)
        {
            int score = 0;

            // Calculate quality score based on the rules provided
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

            // Check for forbidden words
            if (!await ContainsForbiddenWordsAsync(jobDTO.Description))
            {
                score += 2; // 2 points for no forbidden words
            }

            return score; // Returns the total score out of 5
        }
    }
}