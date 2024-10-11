using TalentSpot.Domain.Entities;

namespace TalentSpot.Application.Services
{
    public interface IForbiddenWordService
    {
        Task<IEnumerable<ForbiddenWord>> GetAllForbiddenWordsAsync();
        Task<ForbiddenWord> GetForbiddenWordByIdAsync(Guid id);
        Task AddForbiddenWordAsync(ForbiddenWord forbiddenWord);
        Task UpdateForbiddenWordAsync(ForbiddenWord forbiddenWord);
        Task DeleteForbiddenWordAsync(Guid id);
    }
}