using Elastic.Clients.Elasticsearch.IndexLifecycleManagement;
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

        public async Task<ResponseMessage<JobDTO>> CreateJobAsync(JobCreateDTO jobDTO, Guid userId)
        {
            var company = await _companyRepository.GetCompanyByUserId(userId);
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
            if (jobDTO.BenefitIds != null)
            {
                benefits = (await _benefitRepository.List<Benefit>(p => jobDTO.BenefitIds.Contains(p.Id))).ToList();
                if (benefits == null || !benefits.Any())
                {
                    return ResponseMessage<JobDTO>.FailureResponse("Geçersiz yan haklar.");
                }
            };

            var jobId = Guid.NewGuid();
            var job = new Job
            {
                Id = jobId,
                Position = jobDTO.Position,
                Description = jobDTO.Description,
                ExpirationDate = DateTime.UtcNow.AddDays(15),
                QualityScore = await CalculateQualityScoreAsync(jobDTO.WorkTypeId, jobDTO.Salary, jobDTO.BenefitIds, jobDTO.Description),
                WorkTypeId = jobDTO.WorkTypeId, // WorkType ilişkilendirildi
                Salary = jobDTO.Salary,
                CompanyId = company.Id,
                JobBenefits = benefits.Count() > 0 ? benefits.Select(b => new JobBenefit
                {
                    JobId = jobId,
                    BenefitId = b.Id
                }).ToList() : new List<JobBenefit>()
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
                    Benefits = benefits != null && benefits.Any() ? benefits.Select(p => new BenefitDTO()
                    {
                        Id = p.Id,
                        Name = p.Name
                    }).ToList() : new List<BenefitDTO>(),
                    WorkType = job.WorkType != null ? new WorkTypeDTO()
                    {
                        Id = job.WorkType.Id,
                        Name = job.WorkType.Name,
                    } : new WorkTypeDTO(),
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
            var job = await _jobRepository.GetById<Job>(id, true);
            if (job == null)
            {
                return ResponseMessage<JobDTO>.FailureResponse("İlan bulunamadı.");
            }

            var includes = new List<string> { $"{nameof(JobBenefit.Benefit)}" };
            var benefits = (await _jobBenefitRepository.List<JobBenefit>(p => p.JobId == job.Id, true, includes)).ToList();

            return ResponseMessage<JobDTO>.SuccessResponse(new JobDTO
            {
                Id = job.Id,
                Position = job.Position,
                Description = job.Description,
                ExpirationDate = job.ExpirationDate,
                QualityScore = job.QualityScore,
                Benefits = benefits.Select(p => new BenefitDTO()
                {
                    Id = p.Benefit.Id,
                    Name = p.Benefit.Name
                }).ToList(),
                WorkType = job.WorkType != null ? new WorkTypeDTO()
                {
                    Name = job.WorkType.Name,
                    Id = job.WorkType?.Id,

                } : new WorkTypeDTO(),
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
                var includes = new List<string> { $"{nameof(JobBenefit.Benefit)}" };
                var jobBenefits = (await _jobBenefitRepository.List<JobBenefit>(p => p.JobId == job.Id, true, includes)).ToList();
                var workType = await _workTypeRepository.GetByIdAsync((Guid)job.WorkTypeId);

                jobDTOs.Add(new JobDTO
                {
                    Id = job.Id,
                    Position = job.Position,
                    Description = job.Description,
                    ExpirationDate = job.ExpirationDate,
                    QualityScore = job.QualityScore,
                    Benefits = jobBenefits.Select(p => new BenefitDTO()
                    {
                        Id = p.Benefit.Id,
                        Name = p.Benefit.Name
                    }).ToList(),
                    WorkType = new WorkTypeDTO()
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

        public async Task<ResponseMessage<JobDTO>> UpdateJobAsync(JobUpdateDTO jobUpdateDTO)
        {
            var existingJob = await _jobRepository.GetByIdAsync(jobUpdateDTO.Id);
            if (existingJob == null)
            {
                return ResponseMessage<JobDTO>.FailureResponse("İlan bulunamadı.");
            }

            if (jobUpdateDTO.WorkTypeId != null)
            {
                var workType = await _workTypeRepository.GetByIdAsync((Guid)jobUpdateDTO.WorkTypeId);
                if (workType == null)
                {
                    return ResponseMessage<JobDTO>.FailureResponse("Geçersiz çalışma türü.");
                }
            }

            // Benefits checks
            var benefits = (await _benefitRepository.List<Benefit>(p => jobUpdateDTO.BenefitIds.Contains(p.Id))).ToList();
            if (benefits == null || !benefits.Any())
            {
                return ResponseMessage<JobDTO>.FailureResponse("Geçersiz yan haklar.");
            }

            // Job updates
            existingJob.Position = jobUpdateDTO.Position;
            existingJob.Description = jobUpdateDTO.Description;
            existingJob.WorkTypeId = jobUpdateDTO.WorkTypeId;
            existingJob.Salary = jobUpdateDTO.Salary;
            existingJob.QualityScore = await CalculateQualityScoreAsync(jobUpdateDTO.WorkTypeId, jobUpdateDTO.Salary, jobUpdateDTO.BenefitIds, jobUpdateDTO.Description);
            existingJob.JobBenefits = jobUpdateDTO.BenefitIds.Select(p => new JobBenefit
            {
                JobId = jobUpdateDTO.Id,
                BenefitId = p
            }).ToList();

            // JobBenefits updates
            var deletedJobBenefits = (await _jobBenefitRepository.List<JobBenefit>(p => p.JobId == jobUpdateDTO.Id)).ToList();
            await _jobBenefitRepository.DeleteRangeAsync(deletedJobBenefits);


            await _jobRepository.UpdateAsync(existingJob);
            await _unitOfWork.CompleteAsync();

            return ResponseMessage<JobDTO>.SuccessResponse(null);
        }

        public async Task<bool> DeleteJobAsync(Guid id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null)
            {
                return false;
            }

            await _jobRepository.DeleteAsync(id);
            return await _unitOfWork.CompleteAsync();
        }

        public async Task<ResponseMessage<List<JobDTO>>> SearchJobsByExpirationDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var includes = new List<string>
            {
                $"{nameof(Job.Company)}",
                $"{nameof(Job.JobBenefits)}",
                $"{nameof(Job.JobBenefits)}.{nameof(JobBenefit.Benefit)}",
                $"{nameof(Job.WorkType)}"
            };

            var jobs = (await _jobRepository.List<Job>(j => j.ExpirationDate.Date >= startDate.Date && j.ExpirationDate.Date <= endDate.Date, true, includes)).ToList();

            var jobDTOs = jobs.Select(job => new JobDTO
            {
                Id = job.Id,
                Position = job.Position,
                Description = job.Description,
                ExpirationDate = job.ExpirationDate,
                QualityScore = job.QualityScore,
                Benefits = job.JobBenefits != null
                ? job.JobBenefits.Select(p => new BenefitDTO()
                {
                    Name = p.Benefit?.Name ?? string.Empty,
                    Id = p.Benefit?.Id ?? Guid.Empty
                }).ToList()
                : new List<BenefitDTO>(),
                WorkType = job.WorkType != null
                    ? new WorkTypeDTO
                    {
                        Name = job.WorkType.Name ?? string.Empty,
                        Id = job.WorkType?.Id ?? Guid.Empty
                    }
                    : new WorkTypeDTO(),
                Salary = job.Salary,
                CompanyId = job.CompanyId,
                Company = new CompanyDTO()
                {
                    Id = job.Company?.Id ?? Guid.Empty,
                    Address = job.Company?.Address ?? string.Empty,
                    Name = job.Company?.Name ?? string.Empty,
                }
            }).ToList();

            if (jobDTOs.Any())
            {
                return ResponseMessage<List<JobDTO>>.SuccessResponse(jobDTOs);
            }

            return ResponseMessage<List<JobDTO>>.FailureResponse("Belirtilen tarih aralığında aktif iş ilanı bulunamadı.");
        }

        private async Task<bool> ContainsForbiddenWordsAsync(string description)
        {
            var forbiddenWords = await _forbiddenWordsService.GetAllForbiddenWordsAsync();
            return forbiddenWords.Select(p => p.Word).Any(word => description.Contains(word, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<int> CalculateQualityScoreAsync(Guid? workTypeId, int? salary, List<Guid>? benefitIds, string description)
        {
            int score = 0;

            if (workTypeId != null)
            {
                score++;
            }

            if (salary.HasValue)
            {
                score++;
            }

            if (benefitIds != null)
            {
                score++;
            }

            if (!await ContainsForbiddenWordsAsync(description))
            {
                score += 2;
            }

            return score;
        }
    }
}