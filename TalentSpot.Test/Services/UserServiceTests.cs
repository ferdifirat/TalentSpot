using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using TalentSpot.Application.Constants;
using TalentSpot.Application.DTOs;
using TalentSpot.Application.Services.Concrete;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;
using TalentSpot.Infrastructure.Helper;
using TalentSpot.Infrastructure.Interfaces;

namespace TalentSpot.Test.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ICompanyRepository> _companyRepositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _companyRepositoryMock = new Mock<ICompanyRepository>();
            _cacheServiceMock = new Mock<ICacheService>();
            _userService = new UserService(
                _userRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _companyRepositoryMock.Object,
                _cacheServiceMock.Object,
                "YourVeryLongSecretKeyThatIsAtLeast32Characters" // JWT Secret
            );
        }

        [Fact]
        public async Task RegisterUserWithCompanyAsync_ShouldReturnSuccess_WhenUserIsRegistered()
        {
            // Arrange
            var userDto = new UserCreateDTO
            {
                PhoneNumber = "1234567890",
                Password = "password",
                Email = "test@example.com",
                CompanyName = "Test Company",
                Address = "123 Test St"
            };

            _userRepositoryMock.Setup(repo => repo.GetByPhoneNumberAsync(userDto.PhoneNumber)).ReturnsAsync((User)null);
            _unitOfWorkMock.Setup(uow => uow.BeginTransactionAsync()).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(true);
            _unitOfWorkMock.Setup(uow => uow.CommitAsync()).Returns(Task.CompletedTask);
            _companyRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Company>())).Returns(Task.CompletedTask);
            _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            var result = await _userService.RegisterUserWithCompanyAsync(userDto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(userDto.PhoneNumber, result.Data.PhoneNumber);
            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
            _companyRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Company>()), Times.Once);
        }

        [Fact]
        public async Task RegisterUserWithCompanyAsync_ShouldReturnFailure_WhenUserAlreadyExists()
        {
            // Arrange
            var userDto = new UserCreateDTO { PhoneNumber = "1234567890", Password = "Password123", CompanyName = "Test Company" };
            var existingUser = new User { Id = Guid.NewGuid(), PhoneNumber = userDto.PhoneNumber };

            _userRepositoryMock.Setup(repo => repo.GetByPhoneNumberAsync(userDto.PhoneNumber)).ReturnsAsync(existingUser);

            // Act
            var response = await _userService.RegisterUserWithCompanyAsync(userDto);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be(ResponseMessageConstants.UserAlreadyExists);
            response.Data.Should().BeNull();
        }

        [Fact]
        public async Task RegisterUserWithCompanyAsync_ShouldReturnFailure_WhenExceptionOccursDuringUserCreation()
        {
            // Arrange
            var userDto = new UserCreateDTO { PhoneNumber = "1234567890", Password = "Password123", CompanyName = "Test Company" };
            _userRepositoryMock.Setup(repo => repo.GetByPhoneNumberAsync(userDto.PhoneNumber)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>())).ThrowsAsync(new Exception("User creation failed."));

            // Act
            var response = await _userService.RegisterUserWithCompanyAsync(userDto);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be(ResponseMessageConstants.RegistrationFailed); // Since the catch block has an empty response
            response.Data.Should().BeNull();
        }

        [Fact]
        public async Task RegisterUserWithCompanyAsync_ShouldReturnFailure_WhenExceptionOccursDuringCompanyCreation()
        {
            // Arrange
            var userDto = new UserCreateDTO { PhoneNumber = "1234567890", Password = "Password123", CompanyName = "Test Company" };
            var user = new User { Id = Guid.NewGuid(), PhoneNumber = userDto.PhoneNumber };

            _userRepositoryMock.Setup(repo => repo.GetByPhoneNumberAsync(userDto.PhoneNumber)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(uow => uow.CompleteAsync()).ReturnsAsync(true);
            _companyRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Company>())).ThrowsAsync(new Exception("Company creation failed."));

            // Act
            var response = await _userService.RegisterUserWithCompanyAsync(userDto);

            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.Message.Should().Be(ResponseMessageConstants.RegistrationFailed); // Since the catch block has an empty response
            response.Data.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenUserIsValid()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                PhoneNumber = "1234567890",
                PasswordHash = PasswordHasher.HashPassword("password")
            };

            _userRepositoryMock.Setup(repo => repo.GetByPhoneNumberAsync(user.PhoneNumber)).ReturnsAsync(user);
            _cacheServiceMock.Setup(c => c.SetStringAsync(user.Id.ToString(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>())).Returns(Task.CompletedTask);

            // Act
            var token = await _userService.LoginAsync(user.PhoneNumber, "password");

            // Assert
            Assert.NotNull(token);
            _cacheServiceMock.Verify(c => c.SetStringAsync(user.Id.ToString(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_ShouldRemoveToken_WhenUserLogsOut()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = _userService.GenerateJwtToken(new User { Id = userId });

            // Act
            await _userService.LogoutAsync(token);

            // Assert
            _cacheServiceMock.Verify(c => c.RemoveAsync(userId.ToString()), Times.Once);
        }
    }
}