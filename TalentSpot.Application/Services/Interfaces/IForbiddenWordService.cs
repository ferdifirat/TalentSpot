using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;

namespace TalentSpot.Application.Services
{
    public interface IForbiddenWordService
    {
        Task<IEnumerable<ForbiddenWord>> GetAllForbiddenWordsAsync();
        Task<ForbiddenWord> GetForbiddenWordByIdAsync(Guid id);
        Task<ResponseMessage<ForbiddenWord>> AddForbiddenWordAsync(ForbiddenWord forbiddenWord);
        Task<ResponseMessage<ForbiddenWord>> UpdateForbiddenWordAsync(ForbiddenWord forbiddenWord);
        Task DeleteForbiddenWordAsync(Guid id);
    }
}