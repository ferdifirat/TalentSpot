using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;

namespace TalentSpot.Application.Services.Concrete
{
    public class ForbiddenWordService : IForbiddenWordService
    {
        private readonly IForbiddenWordRepository _forbiddenWordRepository;
        private readonly IDistributedCache _cache;
        private readonly IUnitOfWork _unitOfWork;

        public ForbiddenWordService(IForbiddenWordRepository forbiddenWordRepository, IDistributedCache cache, IUnitOfWork unitOfWork)
        {
            _forbiddenWordRepository = forbiddenWordRepository;
            _cache = cache;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ForbiddenWord>> GetAllForbiddenWordsAsync()
        {
            var cacheKey = "forbiddenwords";
            var forbiddenWordsJson = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(forbiddenWordsJson))
            {
                return JsonSerializer.Deserialize<IEnumerable<ForbiddenWord>>(forbiddenWordsJson);
            }

            var forbiddenWords = await _forbiddenWordRepository.GetAllAsync();
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(forbiddenWords));

            return forbiddenWords;
        }

        public async Task<ForbiddenWord> GetForbiddenWordByIdAsync(Guid id)
        {
            var cacheKey = $"forbiddenword-{id}";
            var forbiddenWordJson = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(forbiddenWordJson))
            {
                return JsonSerializer.Deserialize<ForbiddenWord>(forbiddenWordJson);
            }

            var forbiddenWord = await _forbiddenWordRepository.GetByIdAsync(id);
            if (forbiddenWord != null)
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(forbiddenWord));
            }

            return forbiddenWord;
        }

        public async Task<ResponseMessage<ForbiddenWord>> AddForbiddenWordAsync(ForbiddenWord forbiddenWord)
        {
            var existingWord = (await _forbiddenWordRepository.List<ForbiddenWord>(fw => fw.Word.ToLower() == forbiddenWord.Word.ToLower())).ToList();

            if (existingWord != null)
            {
                return ResponseMessage<ForbiddenWord>.FailureResponse("Yasaklı kelime zaten mevcut.");
            }

            await _forbiddenWordRepository.AddAsync(forbiddenWord);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync("forbiddenwords");
            return ResponseMessage<ForbiddenWord>.SuccessResponse(forbiddenWord);
        }

        public async Task<ResponseMessage<ForbiddenWord>> UpdateForbiddenWordAsync(ForbiddenWord forbiddenWord)
        {
            var existingWord = (await _forbiddenWordRepository.List<ForbiddenWord>(fw => fw.Word.ToLower() == forbiddenWord.Word.ToLower() && fw.Id != forbiddenWord.Id)).ToList();

            if (existingWord != null)
            {
                return ResponseMessage<ForbiddenWord>.FailureResponse("Aynı kelime zaten başka bir kayıt olarak mevcut.");
            }

            await _forbiddenWordRepository.UpdateAsync(forbiddenWord);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync("forbiddenwords");
            await _cache.RemoveAsync($"forbiddenword-{forbiddenWord.Id}");
            return ResponseMessage<ForbiddenWord>.SuccessResponse(forbiddenWord);
        }

        public async Task DeleteForbiddenWordAsync(Guid id)
        {
            await _forbiddenWordRepository.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync("forbiddenwords");
            await _cache.RemoveAsync($"forbiddenword-{id}");
        }
    }
}