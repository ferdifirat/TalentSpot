using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;

namespace TalentSpot.Application.Services.Concrete
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IForbiddenWordService _forbiddenWordsService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkTypeRepository _workTypeRepository;
        private readonly IBenefitRepository _benefitRepository;
        private readonly IJobBenefitRepository _jobBenefitRepository;

        public JobService(IJobRepository jobRepository,
            ICompanyRepository companyRepository,
            IForbiddenWordService forbiddenWordsService,
            IUnitOfWork unitOfWork,
            IWorkTypeRepository workTypeRepository,
            IBenefitRepository benefitRepository,
            IJobBenefitRepository jobBenefitRepository)
        {
            _jobRepository = jobRepository;
            _companyRepository = companyRepository;
            _forbiddenWordsService = forbiddenWordsService;
            _unitOfWork = unitOfWork;
            _workTypeRepository = workTypeRepository;
            _benefitRepository = benefitRepository;
            _jobBenefitRepository = jobBenefitRepository;
        }

        public async Task<ResponseMessage<JobDTO>> CreateJobAsync(JobCreateDTO jobDTO)
        {
            var company = await _companyRepository.GetByIdAsync(jobDTO.CompanyId);
            if (company == null)
            {
                return ResponseMessage<JobDTO>.FailureResponse("Şirket bulunamadı.");
            }

            if (company.AllowedJobPostings == 0)
            {
                return ResponseMessage<JobDTO>.FailureResponse("Bu şirketin ilan yayınlama hakkı kalmamıştır.");
            }

            if (jobDTO.WorkTypeId.HasValue)
            {
                var workType = await _workTypeRepository.GetByIdAsync((Guid)jobDTO.WorkTypeId);
                if (workType == null)
                {
                    return ResponseMessage<JobDTO>.FailureResponse("Geçersiz çalışma türü.");
                }
            }

            var benefits = new List<Benefit>();
            if (jobDTO.BenefitIds.Count() > 0)
            {
                benefits = (await _benefitRepository.FindAsync(p=> jobDTO.BenefitIds.Contains(p.Id))).ToList();
                if (benefits == null || !benefits.Any())
                {
                    return ResponseMessage<JobDTO>.FailureResponse("Geçersiz yan haklar.");
                }
            }

            var jobId = Guid.NewGuid();
            var job = new Job
            {
                Id = jobId,
                Position = jobDTO.Position,
                Description = jobDTO.Description,
                ExpirationDate = DateTime.UtcNow.AddDays(15),
                QualityScore = await CalculateQualityScoreAsync(jobDTO),
                WorkTypeId = jobDTO.WorkTypeId, // WorkType ilişkilendirildi
                Salary = jobDTO.Salary,
                CompanyId = jobDTO.CompanyId,
                JobBenefits = benefits.Count() > 0 ? benefits.Select(b => new JobBenefit
                {
                    JobId = jobId,
                    BenefitId = b.Id
                }).ToList() : null
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
                    //Benefits = string.Join(", ", benefits.Select(b => b.Name)), // Yan haklar string'e dönüştürüldü
                    //WorkType = workType.Name, // WorkType adı alındı
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
                return ResponseMessage<JobDTO>.FailureResponse("İlan bulunamadı.");
            }

            var benefits = await _jobBenefitRepository.GetBenefitsByJobIdAsync(job.Id);
            var workType = await _workTypeRepository.GetByIdAsync((Guid)job.WorkTypeId);

            return ResponseMessage<JobDTO>.SuccessResponse(new JobDTO
            {
                Id = job.Id,
                Position = job.Position,
                Description = job.Description,
                ExpirationDate = job.ExpirationDate,
                QualityScore = job.QualityScore,
                Benefits = benefits.Select(p=>p.Benefit).ToList(),
                WorkType = new WorkType() 
                {
                     Name = workType.Name,
                     Id = job.Id,
                },
                Salary = job.Salary,
                CompanyId = job.CompanyId
            });
        }

        public async Task<ResponseMessage<List<JobDTO>>> GetAllJobAsync()
        {
            var jobs = await _jobRepository.GetAllAsync();
            if (jobs == null || !jobs.Any())
            {
                return ResponseMessage<List<JobDTO>>.FailureResponse("İlan bulunamadı.");
            }

            var jobDTOs = new List<JobDTO>();

            foreach (var job in jobs)
            {
                var benefits = await _jobBenefitRepository.GetBenefitsByJobIdAsync(job.Id);
                var workType = await _workTypeRepository.GetByIdAsync((Guid)job.WorkTypeId);

                jobDTOs.Add(new JobDTO
                {
                    Id = job.Id,
                    Position = job.Position,
                    Description = job.Description,
                    ExpirationDate = job.ExpirationDate,
                    QualityScore = job.QualityScore,
                    Benefits = benefits.Select(p => p.Benefit).ToList(),
                    WorkType = new WorkType()
                    {
                        Id = workType.Id,
                        Name = workType.Name
                    },
                    Salary = job.Salary,
                    CompanyId = job.CompanyId
                });
            }

            return ResponseMessage<List<JobDTO>>.SuccessResponse(jobDTOs);
        }

        public async Task<bool> UpdateJobAsync(JobUpdateDTO jobUpdateDTO)
        {
            var existingJob = await _jobRepository.GetByIdAsync(jobUpdateDTO.Id);
            if (existingJob == null)
            {
                //return ResponseMessage<JobDTO>.FailureResponse("İlan bulunamadı.");
            }

            // WorkType kontrolü
            var workType = await _workTypeRepository.GetByIdAsync(jobUpdateDTO.WorkTypeId);
            if (workType == null)
            {
                //return ResponseMessage<JobDTO>.FailureResponse("Geçersiz çalışma türü.");
            }

            // Benefits kontrolü
            //var benefits = await _benefitRepository.GetByIdsAsync(jobUpdateDTO.BenefitIds);
            //if (benefits == null || !benefits.Any())
            //{
            //    //return ResponseMessage<JobDTO>.FailureResponse("Geçersiz yan haklar.");
            //}

            // Job güncellemesi
            existingJob.Position = jobUpdateDTO.Position;
            existingJob.Description = jobUpdateDTO.Description;
            existingJob.WorkTypeId = jobUpdateDTO.WorkTypeId;
            existingJob.Salary = jobUpdateDTO.Salary;

            // JobBenefits güncellemesi
            //await _jobBenefitRepository.DeleteByJobIdAsync(jobUpdateDTO.Id);
            //foreach (var benefit in benefits)
            //{
            //    await _jobBenefitRepository.AddAsync(new JobBenefit
            //    {
            //        JobId = jobUpdateDTO.Id,
            //        BenefitId = benefit.Id
            //    });
            //}

            await _jobRepository.UpdateAsync(existingJob);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> DeleteJobAsync(Guid id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                //return ResponseMessage<List<JobDTO>>.FailureResponse("İlan bulunamadı.");
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
                var benefits = await _jobBenefitRepository.GetBenefitsByJobIdAsync(job.Id);
                //var workType = await _workTypeRepository.GetByIdAsync(job.WorkTypeId);

                jobDTOs.Add(new JobDTO
                {
                    Id = job.Id,
                    Position = job.Position,
                    Description = job.Description,
                    ExpirationDate = job.ExpirationDate,
                    QualityScore = job.QualityScore,
                    //Benefits = string.Join(", ", benefits.Select(b => b.Name)),
                    //WorkType = workType?.Name ?? "Tanımsız",
                    Salary = job.Salary,
                    CompanyId = job.CompanyId
                });
            }

            return ResponseMessage<List<JobDTO>>.SuccessResponse(jobDTOs);
        }

        private async Task<bool> ContainsForbiddenWordsAsync(string description)
        {
            var forbiddenWords = await _forbiddenWordsService.GetAllForbiddenWordsAsync();
            return forbiddenWords.Select(p=>p.Word).Any(word => description.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<int> CalculateQualityScoreAsync(JobCreateDTO jobDTO)
        {
            int score = 0;

            if (jobDTO.WorkTypeId != null)
            {
                score++;
            }

            if (jobDTO.Salary.HasValue)
            {
                score++;
            }

            if (jobDTO.BenefitIds.Count() > 0)
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