using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Linq.Expressions;
using System.Text.Json;
using TalentSpot.Application.Constants;
using TalentSpot.Application.Services.Concrete;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;
using TalentSpot.Infrastructure.Interfaces;

namespace TalentSpot.Test.Services
{
    public class BenefitServiceTests
    {
        private readonly Mock<IBenefitRepository> _benefitRepositoryMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly BenefitService _benefitService;
        public readonly string _cacheKey = RedisCacheConstants.BenefitCacheKey;

        public BenefitServiceTests()
        {
            _benefitRepositoryMock = new Mock<IBenefitRepository>();
            _cacheMock = new Mock<ICacheService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _benefitService = new BenefitService(_benefitRepositoryMock.Object, _cacheMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetAllBenefitsAsync_ShouldReturnCachedBenefits_WhenCacheIsAvailable()
        {
            // Arrange
            var cachedBenefits = new List<Benefit>
            {
                new Benefit { Id = Guid.NewGuid(), Name = "Benefit1" },
                new Benefit { Id = Guid.NewGuid(), Name = "Benefit2" }
            };

            var cachedJson = JsonSerializer.Serialize(cachedBenefits);
            _cacheMock.Setup(c => c.GetStringAsync(_cacheKey)).ReturnsAsync(cachedJson);

            // Act
            var result = await _benefitService.GetAllBenefitsAsync();

            // Assert
            Assert.Equal(cachedBenefits.Count, result.Count());
            Assert.Equal(cachedBenefits.First().Name, result.First().Name);
            _benefitRepositoryMock.Verify(r => r.GetAllAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllBenefitsAsync_ShouldReturnBenefits_WhenCacheIsNotAvailable()
        {
            // Arrange
            var benefits = new List<Benefit>
            {
                new Benefit { Id = Guid.NewGuid(), Name = "Benefit1" },
                new Benefit { Id = Guid.NewGuid(), Name = "Benefit2" }
            };

            _cacheMock.Setup(c => c.GetStringAsync(_cacheKey)).ReturnsAsync((string)null);
            _benefitRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(benefits);

            // Act
            var result = await _benefitService.GetAllBenefitsAsync();

            // Assert
            Assert.Equal(benefits.Count, result.Count());
            _cacheMock.Verify(c => c.SetStringAsync(_cacheKey, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AddBenefitAsync_ShouldReturnSuccess_WhenBenefitIsNew()
        {
            // Arrange
            var newBenefit = new Benefit { Id = Guid.NewGuid(), Name = "NewBenefit" };
            _benefitRepositoryMock.Setup(r => r.List<Benefit>(It.IsAny<Expression<Func<Benefit, bool>>>(), false, null))
                .ReturnsAsync(new List<Benefit>()); // No existing benefit

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(true);

            // Act
            var result = await _benefitService.AddBenefitAsync(newBenefit);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(newBenefit, result.Result);
            _benefitRepositoryMock.Verify(r => r.AddAsync(newBenefit), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync(_cacheKey), Times.Once);
        }

        [Fact]
        public async Task AddBenefitAsync_ShouldReturnFailure_WhenBenefitExists()
        {
            // Arrange
            var existingBenefit = new Benefit { Id = Guid.NewGuid(), Name = "ExistingBenefit" };
            var newBenefit = new Benefit { Id = Guid.NewGuid(), Name = "ExistingBenefit" }; // Same name

            _benefitRepositoryMock.Setup(r => r.List<Benefit>(It.IsAny<Expression<Func<Benefit, bool>>>(), false, null))
                .ReturnsAsync(new List<Benefit> { existingBenefit });

            // Act
            var result = await _benefitService.AddBenefitAsync(newBenefit);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ResponseMessages.BenefitAlreadyExists, result.Message);
        }

        [Fact]
        public async Task UpdateBenefitAsync_ShouldReturnSuccess_WhenBenefitIsUpdated()
        {
            // Arrange
            var existingBenefit = new Benefit { Id = Guid.NewGuid(), Name = "ExistingBenefit" };
            var updatedBenefit = new Benefit { Id = existingBenefit.Id, Name = "UpdatedBenefit" };

            _benefitRepositoryMock.Setup(r => r.List<Benefit>(It.IsAny<Expression<Func<Benefit, bool>>>(), false, null))
                .ReturnsAsync(new List<Benefit>()); // No existing benefit with the same name

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(true);

            // Act
            var result = await _benefitService.UpdateBenefitAsync(updatedBenefit);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(updatedBenefit, result.Result);
            _benefitRepositoryMock.Verify(r => r.UpdateAsync(updatedBenefit), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync(_cacheKey), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync($"{_cacheKey}-{updatedBenefit.Id}"), Times.Once);
        }

        [Fact]
        public async Task UpdateBenefitAsync_ShouldReturnFailure_WhenSameBenefitExists()
        {
            // Arrange
            var existingBenefit = new Benefit { Id = Guid.NewGuid(), Name = "ExistingBenefit" };
            var updatedBenefit = new Benefit { Id = existingBenefit.Id, Name = "ExistingBenefit" }; // Same name

            _benefitRepositoryMock.Setup(r => r.List<Benefit>(It.IsAny<Expression<Func<Benefit, bool>>>(), false, null))
                .ReturnsAsync(new List<Benefit> { existingBenefit });

            // Act
            var result = await _benefitService.UpdateBenefitAsync(updatedBenefit);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ResponseMessages.DuplicateBenefit, result.Message);
        }

        [Fact]
        public async Task DeleteBenefitAsync_ShouldRemoveBenefit()
        {
            // Arrange
            var benefitId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(true);

            // Act
            await _benefitService.DeleteBenefitAsync(benefitId);

            // Assert
            _benefitRepositoryMock.Verify(r => r.DeleteAsync(benefitId), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync(_cacheKey), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync($"{_cacheKey}-{benefitId}"), Times.Once);
        }
    }
}