﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using TalentSpot.Application.Constants;
using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;
using TalentSpot.Infrastructure.Helper;
using TalentSpot.Infrastructure.Interfaces;

namespace TalentSpot.Application.Services.Concrete
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICacheService _cache;
        private readonly string _jwtSecret;

        public UserService(IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ICompanyRepository companyRepository,
            ICacheService cache,
            string jwtSecret)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _companyRepository = companyRepository;
            _cache = cache;
            _jwtSecret = jwtSecret;
        }

        public async Task<ResponseMessage<UserDTO>> RegisterUserWithCompanyAsync(UserCreateDTO userDto)
        {
            var existingUser = await _userRepository.GetByPhoneNumberAsync(userDto.PhoneNumber);
            if (existingUser != null)
            {
                return ResponseMessage<UserDTO>.FailureResponse(ResponseMessageConstants.UserAlreadyExists);
            }

            var hashedPassword = PasswordHasher.HashPassword(userDto.Password);
            try
            {
                var newUser = new User
                {
                    PhoneNumber = userDto.PhoneNumber,
                    PasswordHash = hashedPassword,
                    Email = userDto.Email
                };

                await _unitOfWork.BeginTransactionAsync();
                await _userRepository.AddAsync(newUser);
                await _unitOfWork.CompleteAsync();

                var newCompany = new Company
                {
                    UserId = newUser.Id,
                    Name = userDto.CompanyName,
                    Address = userDto.Address,
                    AllowedJobPostings = 2
                };
                await _companyRepository.AddAsync(newCompany);
                await _unitOfWork.CompleteAsync();

                await _unitOfWork.CommitAsync();

                return ResponseMessage<UserDTO>.SuccessResponse(new UserDTO()
                {
                    Email = newUser.Email,
                    Id = newUser.Id,
                    PhoneNumber = newUser.PhoneNumber,
                    Company = new CompanyDTO()
                    {
                        Name = newCompany.Name,
                        Address = newCompany.Address,
                        Id = newCompany.Id,
                    }
                });
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackAsync();
                return ResponseMessage<UserDTO>.FailureResponse(ResponseMessageConstants.RegistrationFailed);
            }
        }

        public async Task<string> LoginAsync(string phoneNumber, string password)
        {
            var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);
            if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException(ResponseMessageConstants.InvalidCredentials);
            }

            var token = GenerateJwtToken(user);

            await _cache.SetStringAsync(user.Id.ToString(), token, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });

            return token;
        }

        public async Task LogoutAsync(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userId = jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
            await _cache.RemoveAsync(userId);
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourVeryLongSecretKeyThatIsAtLeast32Characters"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "yourapp",
                audience: "yourapp",
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}