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
    public class WorkTypeServiceTests
    {
        private readonly Mock<IWorkTypeRepository> _workTypeRepositoryMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly WorkTypeService _workTypeService;
        public readonly string _cacheKey = RedisCacheConstants.WorkTypeCacheKey;

        public WorkTypeServiceTests()
        {
            _workTypeRepositoryMock = new Mock<IWorkTypeRepository>();
            _cacheMock = new Mock<ICacheService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _workTypeService = new WorkTypeService(_workTypeRepositoryMock.Object, _cacheMock.Object, _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetAllWorkTypesAsync_ShouldReturnCachedWorkTypes_WhenCacheIsAvailable()
        {
            // Arrange
            var cachedWorkTypes = new List<WorkType>
            {
                new WorkType { Id = Guid.NewGuid(), Name = "WorkType1" },
                new WorkType { Id = Guid.NewGuid(), Name = "WorkType2" }
            };

            var cachedJson = JsonSerializer.Serialize(cachedWorkTypes);
            _cacheMock.Setup(c => c.GetStringAsync(_cacheKey)).ReturnsAsync(cachedJson);

            // Act
            var result = await _workTypeService.GetAllWorkTypesAsync();

            // Assert
            Assert.Equal(cachedWorkTypes.Count, result.Count());
            Assert.Equal(cachedWorkTypes.First().Name, result.First().Name);
            _workTypeRepositoryMock.Verify(r => r.GetAllAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllWorkTypesAsync_ShouldReturnWorkTypes_WhenCacheIsNotAvailable()
        {
            // Arrange
            var workTypes = new List<WorkType>
            {
                new WorkType { Id = Guid.NewGuid(), Name = "WorkType1" },
                new WorkType { Id = Guid.NewGuid(), Name = "WorkType2" }
            };

            _cacheMock.Setup(c => c.GetStringAsync(_cacheKey)).ReturnsAsync((string)null);
            _workTypeRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(workTypes);

            // Act
            var result = await _workTypeService.GetAllWorkTypesAsync();

            // Assert
            Assert.Equal(workTypes.Count, result.Count());
            _cacheMock.Verify(c => c.SetStringAsync(_cacheKey, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task AddWorkTypeAsync_ShouldReturnSuccess_WhenWorkTypeIsNew()
        {
            // Arrange
            var newWorkType = new WorkType { Id = Guid.NewGuid(), Name = "NewWorkType" };
            _workTypeRepositoryMock.Setup(r => r.List<WorkType>(It.IsAny<Expression<Func<WorkType, bool>>>(), false, null))
                .ReturnsAsync(new List<WorkType>()); // No existing work type

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(true);

            // Act
            var result = await _workTypeService.AddWorkTypeAsync(newWorkType);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(newWorkType, result.Data);
            _workTypeRepositoryMock.Verify(r => r.AddAsync(newWorkType), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync(_cacheKey), Times.Once);
        }

        [Fact]
        public async Task AddWorkTypeAsync_ShouldReturnFailure_WhenWorkTypeExists()
        {
            // Arrange
            var existingWorkType = new WorkType { Id = Guid.NewGuid(), Name = "ExistingWorkType" };
            var newWorkType = new WorkType { Id = Guid.NewGuid(), Name = "ExistingWorkType" };

            _workTypeRepositoryMock.Setup(r => r.List<WorkType>(It.IsAny<Expression<Func<WorkType, bool>>>(), false, null))
                .ReturnsAsync(new List<WorkType> { existingWorkType });

            // Act
            var result = await _workTypeService.AddWorkTypeAsync(newWorkType);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ResponseMessageConstants.WorkTypeAlreadyExists, result.Message);
        }

        [Fact]
        public async Task UpdateWorkTypeAsync_ShouldReturnSuccess_WhenWorkTypeIsUpdated()
        {
            // Arrange
            var existingWorkType = new WorkType { Id = Guid.NewGuid(), Name = "ExistingWorkType" };
            var updatedWorkType = new WorkType { Id = existingWorkType.Id, Name = "UpdatedWorkType" };

            _workTypeRepositoryMock.Setup(r => r.List<WorkType>(It.IsAny<Expression<Func<WorkType, bool>>>(), false, null))
                .ReturnsAsync(new List<WorkType>()); // No existing work type with the same name

            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(true);

            // Act
            var result = await _workTypeService.UpdateWorkTypeAsync(updatedWorkType);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(updatedWorkType, result.Data);
            _workTypeRepositoryMock.Verify(r => r.UpdateAsync(updatedWorkType), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync(_cacheKey), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync($"{_cacheKey}-{updatedWorkType.Id}"), Times.Once);
        }

        [Fact]
        public async Task UpdateWorkTypeAsync_ShouldReturnFailure_WhenSameWorkTypeExists()
        {
            // Arrange
            var existingWorkType = new WorkType { Id = Guid.NewGuid(), Name = "ExistingWorkType" };
            var updatedWorkType = new WorkType { Id = existingWorkType.Id, Name = "ExistingWorkType" }; // Same name

            _workTypeRepositoryMock.Setup(r => r.List<WorkType>(It.IsAny<Expression<Func<WorkType, bool>>>(), false, null))
                .ReturnsAsync(new List<WorkType> { existingWorkType });

            // Act
            var result = await _workTypeService.UpdateWorkTypeAsync(updatedWorkType);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(ResponseMessageConstants.WorkTypeExistsElsewhere, result.Message);
        }

        [Fact]
        public async Task DeleteWorkTypeAsync_ShouldRemoveWorkType()
        {
            // Arrange
            var workTypeId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(true);

            // Act
            await _workTypeService.DeleteWorkTypeAsync(workTypeId);

            // Assert
            _workTypeRepositoryMock.Verify(r => r.DeleteAsync(workTypeId), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync(_cacheKey), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync($"{_cacheKey}-{workTypeId}"), Times.Once);
        }
    }
}