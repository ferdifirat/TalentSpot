using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Interfaces;

namespace TalentSpot.Application.Services
{
    public interface IUserService
    {
        Task<ResponseMessage<UserDTO>> RegisterUserWithCompanyAsync(UserCreateDTO userDto);
        Task<string> LoginAsync(string phoneNumber, string password);
        Task LogoutAsync(string token);
    }
}