using System.Text.Json;
using TalentSpot.Application.Constants;
using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;
using TalentSpot.Infrastructure.Interfaces;

namespace TalentSpot.Application.Services.Concrete
{
    public class ForbiddenWordService : IForbiddenWordService
    {
        private readonly IForbiddenWordRepository _forbiddenWordRepository;
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _cacheKey = RedisCacheConstants.ForbiddenWordCacheKey;

        public ForbiddenWordService(IForbiddenWordRepository forbiddenWordRepository, ICacheService cache, IUnitOfWork unitOfWork)
        {
            _forbiddenWordRepository = forbiddenWordRepository;
            _cache = cache;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ForbiddenWord>> GetAllForbiddenWordsAsync()
        {
            var forbiddenWordsJson = await _cache.GetStringAsync(_cacheKey);

            if (!string.IsNullOrEmpty(forbiddenWordsJson))
            {
                return JsonSerializer.Deserialize<IEnumerable<ForbiddenWord>>(forbiddenWordsJson);
            }

            var forbiddenWords = await _forbiddenWordRepository.GetAllAsync();
            await _cache.SetStringAsync(_cacheKey, JsonSerializer.Serialize(forbiddenWords));

            return forbiddenWords;
        }

        public async Task<ForbiddenWord> GetForbiddenWordByIdAsync(Guid id)
        {
            var cacheKey = $"{_cacheKey}-{id}";
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

            if (existingWord.Any())
            {
                return ResponseMessage<ForbiddenWord>.FailureResponse(ResponseMessageConstants.ForbiddenWordAlreadyExists);
            }

            await _forbiddenWordRepository.AddAsync(forbiddenWord);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync(_cacheKey);
            return ResponseMessage<ForbiddenWord>.SuccessResponse(forbiddenWord);
        }

        public async Task<ResponseMessage<ForbiddenWord>> UpdateForbiddenWordAsync(ForbiddenWord forbiddenWord)
        {
            var existingWord = (await _forbiddenWordRepository.List<ForbiddenWord>(fw => fw.Word.ToLower() == forbiddenWord.Word.ToLower() && fw.Id != forbiddenWord.Id)).ToList();

            if (existingWord.Any())
            {
                return ResponseMessage<ForbiddenWord>.FailureResponse(ResponseMessageConstants.AnotherForbiddenWordExists);
            }

            await _forbiddenWordRepository.UpdateAsync(forbiddenWord);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync(_cacheKey);
            await _cache.RemoveAsync($"{_cacheKey}-{forbiddenWord.Id}");
            return ResponseMessage<ForbiddenWord>.SuccessResponse(forbiddenWord);
        }

        public async Task DeleteForbiddenWordAsync(Guid id)
        {
            await _forbiddenWordRepository.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync(_cacheKey);
            await _cache.RemoveAsync($"{_cacheKey}-{id}");
        }
    }
}