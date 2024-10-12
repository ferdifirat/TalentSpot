using Moq;
using TalentSpot.Application.Constants; // Add this for response messages
using TalentSpot.Application.DTOs;
using TalentSpot.Application.Services.Concrete;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;
using Xunit;

namespace TalentSpot.Test.Services
{
    public class CompanyServiceTests
    {
        private readonly Mock<ICompanyRepository> _mockCompanyRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CompanyService _companyService;

        public CompanyServiceTests()
        {
            _mockCompanyRepository = new Mock<ICompanyRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _companyService = new CompanyService(_mockCompanyRepository.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetAllCompanyAsync_ReturnsSuccessResponse_WhenCompaniesExist()
        {
            // Arrange
            var companies = new List<Company>
            {
                new Company { Id = Guid.NewGuid(), Name = "Company A", Address = "Address A", AllowedJobPostings = 5, User = new User { Id = Guid.NewGuid(), PhoneNumber = "123456789" }},
                new Company { Id = Guid.NewGuid(), Name = "Company B", Address = "Address B", AllowedJobPostings = 10, User = new User { Id = Guid.NewGuid(), PhoneNumber = "987654321" }}
            };

            _mockCompanyRepository.Setup(repo => repo.List<Company>(true, It.IsAny<List<string>>())).ReturnsAsync(companies);

            // Act
            var result = await _companyService.GetAllCompanyAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetAllCompanyAsync_ReturnsFailureResponse_WhenNoCompaniesExist()
        {
            // Arrange
            _mockCompanyRepository.Setup(repo => repo.List<Company>(true, It.IsAny<List<string>>())).ReturnsAsync(new List<Company>());

            // Act
            var result = await _companyService.GetAllCompanyAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ResponseMessageConstants.JobNotFound, result.Message); // Use constant here
        }

        [Fact]
        public async Task GetCompanyAsync_ReturnsSuccessResponse_WhenCompanyExists()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var company = new Company { Id = companyId, Name = "Company A", Address = "Address A", AllowedJobPostings = 5, User = new User { Id = Guid.NewGuid(), PhoneNumber = "123456789" } };

            _mockCompanyRepository.Setup(repo => repo.GetById<Company>(companyId, true)).ReturnsAsync(company);

            // Act
            var result = await _companyService.GetCompanyAsync(companyId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(companyId, result.Data.Id);
        }

        [Fact]
        public async Task GetCompanyAsync_ReturnsFailureResponse_WhenCompanyDoesNotExist()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            _mockCompanyRepository.Setup(repo => repo.GetById<Company>(companyId, true)).ReturnsAsync((Company)null);

            // Act
            var result = await _companyService.GetCompanyAsync(companyId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ResponseMessageConstants.CompanyNotFound, result.Message); // Use constant here
        }

        [Fact]
        public async Task UpdateCompanyAsync_ReturnsSuccessResponse_WhenCompanyUpdated()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var user = new User()
            {
                Id = Guid.NewGuid(),
                PhoneNumber = "55651665161"
            };
            var existingCompany = new Company { Id = companyId, Name = "Old Company", Address = "Old Address" };
            var updateDTO = new CompanyDTO { Id = companyId, Name = "Updated Company", Address = "Updated Address" };

            _mockCompanyRepository.Setup(repo => repo.GetById<Company>(companyId, true)).ReturnsAsync(existingCompany);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(true);

            // Act
            var result = await _companyService.UpdateCompanyAsync(updateDTO);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Updated Company", result.Data.Name);
        }

        [Fact]
        public async Task UpdateCompanyAsync_ReturnsFailureResponse_WhenCompanyDoesNotExist()
        {
            // Arrange
            var updateDTO = new CompanyDTO { Id = Guid.NewGuid(), Name = "Non-existing Company" };
            _mockCompanyRepository.Setup(repo => repo.GetById<Company>(updateDTO.Id, true)).ReturnsAsync((Company)null);

            // Act
            var result = await _companyService.UpdateCompanyAsync(updateDTO);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ResponseMessageConstants.CompanyNotFound, result.Message); // Use constant here
        }

        [Fact]
        public async Task UpdateCompanyAsync_ReturnsFailureResponse_WhenUpdateFails()
        {
            // Arrange
            var companyId = Guid.NewGuid();
            var user = new User()
            {
                Id = Guid.NewGuid(),
                PhoneNumber = "55651665161"
            };
            var existingCompany = new Company { Id = companyId, Name = "Old Company", Address = "Old Address" };
            var updateDTO = new CompanyDTO { Id = companyId, Name = "Updated Company", Address = "Updated Address" };

            _mockCompanyRepository.Setup(repo => repo.GetById<Company>(companyId, true)).ReturnsAsync(existingCompany);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(false);

            // Act
            var result = await _companyService.UpdateCompanyAsync(updateDTO);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ResponseMessageConstants.CompanyUpdateFailed, result.Message); // Use constant here
        }
    }
}