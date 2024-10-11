using TalentSpot.Domain.Entities;

namespace TalentSpot.Domain.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> GetByPhoneNumberAsync(string phoneNumber);
    }
}
