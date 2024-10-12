using Moq;
using System.Linq.Expressions;
using System;
using TalentSpot.Application.DTOs;
using TalentSpot.Application.Services;
using TalentSpot.Application.Services.Concrete;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;
using Xunit;
using FluentAssertions;
using TalentSpot.Application.Constants;
namespace TalentSpot.Test.Services
{
    public class JobServiceTests
    {
        private readonly Mock<IJobRepository> _jobRepositoryMock;
        private readonly Mock<ICompanyRepository> _companyRepositoryMock;
        private readonly Mock<IForbiddenWordService> _forbiddenWordsServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IWorkTypeRepository> _workTypeRepositoryMock;
        private readonly Mock<IBenefitRepository> _benefitRepositoryMock;
        private readonly Mock<IJobBenefitRepository> _jobBenefitRepositoryMock;
        private readonly JobService _jobService;

        public JobServiceTests()
        {
            _jobRepositoryMock = new Mock<IJobRepository>();
            _companyRepositoryMock = new Mock<ICompanyRepository>();
            _forbiddenWordsServiceMock = new Mock<IForbiddenWordService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _workTypeRepositoryMock = new Mock<IWorkTypeRepository>();
            _benefitRepositoryMock = new Mock<IBenefitRepository>();
            _jobBenefitRepositoryMock = new Mock<IJobBenefitRepository>();

            _jobService = new JobService(
                _jobRepositoryMock.Object,
                _companyRepositoryMock.Object,
                _forbiddenWordsServiceMock.Object,
                _unitOfWorkMock.Object,
                _workTypeRepositoryMock.Object,
                _benefitRepositoryMock.Object,
                _jobBenefitRepositoryMock.Object
            );
        }

        [Fact]
        public async Task CreateJobAsync_ShouldReturnSuccessResponse_WhenJobIsCreated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var jobCreateDTO = new JobCreateDTO
            {
                Position = "Software Engineer",
                Description = "Develop software.",
                Salary = 60000,
                WorkTypeId = Guid.NewGuid(),
                BenefitIds = new List<Guid> { Guid.NewGuid() }
            };
            var company = new Company { Id = Guid.NewGuid(), AllowedJobPostings = 1 };
            var workType = new WorkType { Id = jobCreateDTO.WorkTypeId.Value, Name = "Full-Time" };
            var benefit = new Benefit { Id = jobCreateDTO.BenefitIds.First(), Name = "Health Insurance" };

            _companyRepositoryMock.Setup(c => c.GetCompanyByUserId(userId)).ReturnsAsync(company);
            _workTypeRepositoryMock.Setup(w => w.GetByIdAsync(jobCreateDTO.WorkTypeId.Value)).ReturnsAsync(workType);
            _benefitRepositoryMock.Setup(b => b.List<Benefit>(It.IsAny<Expression<Func<Benefit, bool>>>(), false, null)).ReturnsAsync(new List<Benefit> { benefit });
            _jobRepositoryMock.Setup(j => j.AddAsync(It.IsAny<Job>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(true);

            // Act
            var result = await _jobService.CreateJobAsync(jobCreateDTO, userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Result);
            Assert.Equal(jobCreateDTO.Position, result.Result.Position);
            _companyRepositoryMock.Verify(c => c.UpdateAsync(It.IsAny<Company>()), Times.Once);
        }

        [Fact]
        public async Task CreateJobAsync_ShouldReturnFailure_WhenCompanyNotFound()
        {
            // Arrange
            var jobDTO = new JobCreateDTO();
            var userId = Guid.NewGuid();

            _companyRepositoryMock.Setup(repo => repo.GetCompanyByUserId(userId)).ReturnsAsync((Company)null);

            // Act
            var response = await _jobService.CreateJobAsync(jobDTO, userId);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be(ResponseMessages.CompanyNotFound);
            response.Result.Should().BeNull();
        }

        [Fact]
        public async Task CreateJobAsync_ShouldReturnFailure_WhenCompanyAllowedJobPostingsIsZero()
        {
            // Arrange
            var jobDTO = new JobCreateDTO();
            var userId = Guid.NewGuid();
            var company = new Company { Id = Guid.NewGuid(), AllowedJobPostings = 0 };

            _companyRepositoryMock.Setup(repo => repo.GetCompanyByUserId(userId)).ReturnsAsync(company);

            // Act
            var response = await _jobService.CreateJobAsync(jobDTO, userId);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be(ResponseMessages.NoAllowedJobPostings);
            response.Result.Should().BeNull();
        }

        [Fact]
        public async Task CreateJobAsync_ShouldReturnFailure_WhenWorkTypeIsInvalid()
        {
            // Arrange
            var jobDTO = new JobCreateDTO { WorkTypeId = Guid.NewGuid() };
            var userId = Guid.NewGuid();
            var company = new Company { Id = Guid.NewGuid(), AllowedJobPostings = 5 };

            _companyRepositoryMock.Setup(repo => repo.GetCompanyByUserId(userId)).ReturnsAsync(company);
            _workTypeRepositoryMock.Setup(repo => repo.GetByIdAsync(jobDTO.WorkTypeId.Value)).ReturnsAsync((WorkType)null);

            // Act
            var response = await _jobService.CreateJobAsync(jobDTO, userId);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be("Geçersiz çalýþma türü.");
            response.Result.Should().BeNull();
        }

        [Fact]
        public async Task CreateJobAsync_ShouldReturnFailure_WhenBenefitsAreInvalid()
        {
            // Arrange
            var jobDTO = new JobCreateDTO { BenefitIds = new List<Guid> { Guid.NewGuid() } };
            var userId = Guid.NewGuid();
            var company = new Company { Id = Guid.NewGuid(), AllowedJobPostings = 5 };

            _companyRepositoryMock.Setup(repo => repo.GetCompanyByUserId(userId)).ReturnsAsync(company);
            _benefitRepositoryMock.Setup(repo => repo.List<Benefit>(It.IsAny<Expression<Func<Benefit, bool>>>(), false, null))
                .ReturnsAsync(new List<Benefit>()); // No benefits found

            // Act
            var response = await _jobService.CreateJobAsync(jobDTO, userId);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be(ResponseMessages.InvalidBenefits);
            response.Result.Should().BeNull();
        }

        [Fact]
        public async Task CreateJobAsync_ShouldReturnFailure_WhenExceptionOccurs()
        {
            // Arrange
            var jobDTO = new JobCreateDTO { Position = "Developer" };
            var userId = Guid.NewGuid();
            var company = new Company { Id = Guid.NewGuid(), AllowedJobPostings = 5 };
            var workType = new WorkType { Id = Guid.NewGuid(), Name = "Full-Time" };

            _companyRepositoryMock.Setup(repo => repo.GetCompanyByUserId(userId)).ReturnsAsync(company);
            _workTypeRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(workType);
            _benefitRepositoryMock.Setup(repo => repo.List<Benefit>(It.IsAny<Expression<Func<Benefit, bool>>>(), false, null))
                .ReturnsAsync(new List<Benefit> { new Benefit { Id = Guid.NewGuid(), Name = "Health" } });

            _unitOfWorkMock.Setup(uow => uow.BeginTransactionAsync()).ThrowsAsync(new Exception("Database error"));

            // Act
            var response = await _jobService.CreateJobAsync(jobDTO, userId);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Contain("Bir hata oluþtu:");
            response.Result.Should().BeNull();
        }

        [Fact]
        public async Task CreateJobAsync_ShouldReturnSuccess_WhenJobIsCreated()
        {
            // Arrange
            var jobDTO = new JobCreateDTO { Position = "Developer", Salary = 50000 };
            var userId = Guid.NewGuid();
            var company = new Company { Id = Guid.NewGuid(), AllowedJobPostings = 5 };
            var workType = new WorkType { Id = Guid.NewGuid(), Name = "Full-Time" };
            var benefit = new Benefit { Id = Guid.NewGuid(), Name = "Health Insurance" };

            _companyRepositoryMock.Setup(repo => repo.GetCompanyByUserId(userId)).ReturnsAsync(company);
            _workTypeRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(workType);
            _benefitRepositoryMock.Setup(repo => repo.List<Benefit>(It.IsAny<Expression<Func<Benefit, bool>>>(),false, null))
                .ReturnsAsync(new List<Benefit> { benefit });

            _unitOfWorkMock.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _jobRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Job>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(true);
            _companyRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Company>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            var response = await _jobService.CreateJobAsync(jobDTO, userId);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Result.Should().NotBeNull();
            response.Result.Position.Should().Be(jobDTO.Position);
            response.Result.Salary.Should().Be(jobDTO.Salary);
        }


        [Fact]
        public async Task GetJobAsync_ShouldReturnJobDTO_WhenJobExists()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var job = new Job
            {
                Id = jobId,
                Position = "Software Engineer",
                Description = "Develop software.",
                Salary = 60000,
                QualityScore = 3,
                ExpirationDate = DateTime.UtcNow.AddDays(15)
            };

            _jobRepositoryMock.Setup(j => j.GetById<Job>(jobId, true)).ReturnsAsync(job);

            // Act
            var result = await _jobService.GetJobAsync(jobId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Result);
            Assert.Equal(jobId, result.Result.Id);
        }

        [Fact]
        public async Task UpdateJobAsync_ShouldReturnSuccessResponse_WhenJobIsUpdated()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var existingJob = new Job
            {
                Id = jobId,
                Position = "Old Position",
                Description = "Old Description",
                Salary = 50000
            };

            var jobUpdateDTO = new JobUpdateDTO
            {
                Id = jobId,
                Position = "New Position",
                Description = "New Description",
                Salary = 60000,
                WorkTypeId = Guid.NewGuid(),
                BenefitIds = new List<Guid> { Guid.NewGuid() }
            };

            _jobRepositoryMock.Setup(j => j.GetByIdAsync(jobId)).ReturnsAsync(existingJob);
            _workTypeRepositoryMock.Setup(w => w.GetByIdAsync(jobUpdateDTO.WorkTypeId.Value)).ReturnsAsync(new WorkType { Id = jobUpdateDTO.WorkTypeId.Value });
            _benefitRepositoryMock.Setup(b => b.List<Benefit>(It.IsAny<Expression<Func<Benefit, bool>>>(), false, null)).ReturnsAsync(new List<Benefit> { new Benefit { Id = jobUpdateDTO.BenefitIds.First() } });
            _jobBenefitRepositoryMock.Setup(jb => jb.List<JobBenefit>(It.IsAny<Expression<Func<JobBenefit, bool>>>(), false, null)).ReturnsAsync(new List<JobBenefit> { new JobBenefit { JobId = jobId } });
            _jobBenefitRepositoryMock.Setup(jb => jb.DeleteRangeAsync(It.IsAny<List<JobBenefit>>())).Returns(Task.CompletedTask);
            _jobRepositoryMock.Setup(j => j.UpdateAsync(existingJob)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(true);

            // Act
            var result = await _jobService.UpdateJobAsync(jobUpdateDTO);

            // Assert
            Assert.True(result.Success);
            _jobRepositoryMock.Verify(j => j.UpdateAsync(existingJob), Times.Once);
        }

        [Fact]
        public async Task SearchJobsByExpirationDateRangeAsync_ShouldReturnJobs_WhenJobsExistInDateRange()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-10);
            var endDate = DateTime.UtcNow.AddDays(10);
            var includes = new List<string>
            {
                $"{nameof(Job.Company)}",
                $"{nameof(Job.JobBenefits)}",
                $"{nameof(Job.JobBenefits)}.{nameof(JobBenefit.Benefit)}",
                $"{nameof(Job.WorkType)}"
            };

            var jobs = new List<Job>
            {
                new Job
                {
                    Id = Guid.NewGuid(),
                    Position = "Software Engineer",
                    Description = "Develop software",
                    ExpirationDate = DateTime.UtcNow.AddDays(5),
                    QualityScore = 5,
                    Salary = 60000,
                    CompanyId = Guid.NewGuid(),
                    JobBenefits = new List<JobBenefit>()
                }
            };

            _jobRepositoryMock.Setup(repo => repo.List<Job>(It.IsAny<Expression<Func<Job, bool>>>(), true, includes))
                              .ReturnsAsync(jobs);

            // Act
            var response = await _jobService.SearchJobsByExpirationDateRangeAsync(startDate, endDate);

            // Assert
            response.Result.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Result.Should().HaveCount(1);
            response.Result.First().Position.Should().Be("Software Engineer");
        }

        [Fact]
        public async Task SearchJobsByExpirationDateRangeAsync_ShouldReturnFailureResponse_WhenNoJobsExistInDateRange()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(1);
            var endDate = DateTime.UtcNow.AddDays(10);

            _jobRepositoryMock.Setup(repo => repo.List<Job>(It.IsAny<Expression<Func<Job, bool>>>(), true, null))
                              .ReturnsAsync(new List<Job>());

            // Act
            var response = await _jobService.SearchJobsByExpirationDateRangeAsync(startDate, endDate);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be(ResponseMessages.NoActiveJobs);
        }

        [Fact]
        public async Task SearchJobsByExpirationDateRangeAsync_ShouldReturnEmptyList_WhenJobBenefitsAreNull()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-10);
            var endDate = DateTime.UtcNow.AddDays(10);
            var jobs = new List<Job>
            {
                new Job
                {
                    Id = Guid.NewGuid(),
                    Position = "Software Engineer",
                    Description = "Develop software",
                    ExpirationDate = DateTime.UtcNow.AddDays(5),
                    QualityScore = 5,
                    Salary = 60000,
                    CompanyId = Guid.NewGuid(),
                    WorkType = new WorkType()
                    {
                        Name = "Freelance",
                        Id = Guid.NewGuid()
                    },
                    Company = new Company()
                    {
                        Id = Guid.NewGuid(),
                        Name = "Kariyer",
                        Address = "Ýstanbul"
                    }
                }
            };

            var includes = new List<string>
            {
                $"{nameof(Job.Company)}",
                $"{nameof(Job.JobBenefits)}",
                $"{nameof(Job.JobBenefits)}.{nameof(JobBenefit.Benefit)}",
                $"{nameof(Job.WorkType)}"
            };

            _jobRepositoryMock.Setup(repo => repo.List<Job>(It.IsAny<Expression<Func<Job, bool>>>(), true, includes))
                              .ReturnsAsync(jobs);

            // Act
            var response = await _jobService.SearchJobsByExpirationDateRangeAsync(startDate, endDate);

            // Assert
            response.Result.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Result.First().Benefits.Should().BeEmpty(); // Expecting empty benefits
        }

        [Fact]
        public async Task GetAllJobAsync_ShouldReturnSuccessResponse_WhenJobsFound()
        {
            // Arrange
            var jobId = Guid.NewGuid();
            var jobs = new List<Job>
        {
            new Job
            {
                Id = jobId,
                Position = "Software Engineer",
                Description = "Develop software",
                ExpirationDate = DateTime.UtcNow.AddDays(5),
                QualityScore = 5,
                Salary = 60000,
                CompanyId = Guid.NewGuid(),
                WorkTypeId = Guid.NewGuid() // Assign a valid WorkTypeId
            }
        };

            var jobBenefits = new List<JobBenefit>
            {
                new JobBenefit
                {
                    JobId = jobId,
                    Benefit = new Benefit { Id = Guid.NewGuid(), Name = "Health Insurance" }
                }
            };

            var workType = new WorkType { Id = jobs[0].WorkTypeId.Value, Name = "Full-Time" };

            var includes = new List<string> { $"{nameof(JobBenefit.Benefit)}" };

            _jobRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(jobs);
            _jobBenefitRepositoryMock.Setup(repo => repo.List<JobBenefit>(It.IsAny<Expression<Func<JobBenefit, bool>>>(), true, includes))
                .ReturnsAsync(jobBenefits);
            _workTypeRepositoryMock.Setup(repo => repo.GetByIdAsync(jobs[0].WorkTypeId.Value)).ReturnsAsync(workType);

            // Act
            var response = await _jobService.GetAllJobAsync();

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.Result.Should().NotBeNullOrEmpty();
            response.Result.Count.Should().Be(1);

            var jobDTO = response.Result.First();
            jobDTO.Position.Should().Be(jobs[0].Position);
            jobDTO.Description.Should().Be(jobs[0].Description);
            jobDTO.ExpirationDate.Should().Be(jobs[0].ExpirationDate);
            jobDTO.QualityScore.Should().Be(jobs[0].QualityScore);
            jobDTO.Salary.Should().Be(jobs[0].Salary);
            jobDTO.CompanyId.Should().Be(jobs[0].CompanyId);
            jobDTO.WorkType.Name.Should().Be(workType.Name);
            jobDTO.Benefits.Should().HaveCount(1);
            jobDTO.Benefits.First().Name.Should().Be(jobBenefits[0].Benefit.Name);
        }
    }
}