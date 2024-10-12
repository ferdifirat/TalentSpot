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
    public class ForbiddenWordServiceTests
    {
        private readonly Mock<IForbiddenWordRepository> _forbiddenWordRepositoryMock;
        private readonly Mock<ICacheService> _cacheMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ForbiddenWordService _forbiddenWordService;
        public readonly string _cacheKey = RedisCacheConstants.ForbiddenWordCacheKey;

        public ForbiddenWordServiceTests()
        {
            _forbiddenWordRepositoryMock = new Mock<IForbiddenWordRepository>();
            _cacheMock = new Mock<ICacheService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _forbiddenWordService = new ForbiddenWordService(
                _forbiddenWordRepositoryMock.Object,
                _cacheMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetAllForbiddenWordsAsync_ShouldReturnCachedWords_WhenCacheIsNotEmpty()
        {
            // Arrange
            var cachedWords = new List<ForbiddenWord>
            {
                new ForbiddenWord { Id = Guid.NewGuid(), Word = "test" }
            };
            _cacheMock.Setup(c => c.GetStringAsync(_cacheKey))
                .ReturnsAsync(JsonSerializer.Serialize(cachedWords));

            // Act
            var result = await _forbiddenWordService.GetAllForbiddenWordsAsync();

            // Assert
            Assert.Equal(cachedWords.Count, result.Count());
        }

        [Fact]
        public async Task GetAllForbiddenWordsAsync_ShouldReturnWordsFromRepository_WhenCacheIsEmpty()
        {
            // Arrange
            var forbiddenWords = new List<ForbiddenWord>
            {
                new ForbiddenWord { Id = Guid.NewGuid(), Word = "test" }
            };
            _cacheMock.Setup(c => c.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync((string)null);
            _forbiddenWordRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(forbiddenWords);

            // Act
            var result = await _forbiddenWordService.GetAllForbiddenWordsAsync();

            // Assert
            Assert.Equal(forbiddenWords.Count, result.Count());
        }

        [Fact]
        public async Task GetForbiddenWordByIdAsync_ShouldReturnCachedWord_WhenCacheIsNotEmpty()
        {
            // Arrange
            var forbiddenWordId = Guid.NewGuid();
            var cachedWord = new ForbiddenWord { Id = forbiddenWordId, Word = "test" };
            var cacheKey = $"{_cacheKey}-{forbiddenWordId}";
            _cacheMock.Setup(c => c.GetStringAsync(cacheKey))
                .ReturnsAsync(JsonSerializer.Serialize(cachedWord));

            // Act
            var result = await _forbiddenWordService.GetForbiddenWordByIdAsync(forbiddenWordId);

            // Assert
            Assert.Equal(cachedWord.Word, result.Word);
        }

        [Fact]
        public async Task AddForbiddenWordAsync_ShouldReturnSuccess_WhenWordIsNew()
        {
            // Arrange
            var forbiddenWord = new ForbiddenWord { Id = Guid.NewGuid(), Word = "newword" };
            _forbiddenWordRepositoryMock.Setup(r => r.List<ForbiddenWord>(It.IsAny<Expression<Func<ForbiddenWord, bool>>>(), false, null))
            .ReturnsAsync(new List<ForbiddenWord>());
            _forbiddenWordRepositoryMock.Setup(r => r.AddAsync(forbiddenWord))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(true);

            // Act
            var result = await _forbiddenWordService.AddForbiddenWordAsync(forbiddenWord);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(forbiddenWord, result.Result);
            _cacheMock.Verify(c => c.RemoveAsync(_cacheKey), Times.Once);
        }

        [Fact]
        public async Task UpdateForbiddenWordAsync_ShouldReturnSuccess_WhenWordIsUpdated()
        {
            // Arrange
            var forbiddenWord = new ForbiddenWord { Id = Guid.NewGuid(), Word = "updatedword" };
            _forbiddenWordRepositoryMock.Setup(r => r.List<ForbiddenWord>(It.IsAny<Expression<Func<ForbiddenWord, bool>>>(), false, null))
                .ReturnsAsync(new List<ForbiddenWord>()); // Düzeltme
            _forbiddenWordRepositoryMock.Setup(r => r.UpdateAsync(forbiddenWord))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(true);

            // Act
            var result = await _forbiddenWordService.UpdateForbiddenWordAsync(forbiddenWord);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(forbiddenWord, result.Result);
            _cacheMock.Verify(c => c.RemoveAsync(_cacheKey), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync($"{_cacheKey}-{forbiddenWord.Id}"), Times.Once);
        }

        [Fact]
        public async Task DeleteForbiddenWordAsync_ShouldCallRepositoryAndClearCache()
        {
            // Arrange
            var forbiddenWordId = Guid.NewGuid();
            _forbiddenWordRepositoryMock.Setup(r => r.DeleteAsync(forbiddenWordId))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(true);

            // Act
            await _forbiddenWordService.DeleteForbiddenWordAsync(forbiddenWordId);

            // Assert
            _forbiddenWordRepositoryMock.Verify(r => r.DeleteAsync(forbiddenWordId), Times.Once);
            _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync(_cacheKey), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync($"{_cacheKey}-{forbiddenWordId}"), Times.Once);
        }
    }
}