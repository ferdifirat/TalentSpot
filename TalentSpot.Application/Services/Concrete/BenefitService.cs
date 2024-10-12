using System.Text.Json;
using TalentSpot.Application.Constants;
using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;
using TalentSpot.Infrastructure.Interfaces;

namespace TalentSpot.Application.Services.Concrete
{
    public class BenefitService : IBenefitService
    {
        private readonly IBenefitRepository _benefitRepository;
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _cacheKey = RedisCacheConstants.BenefitCacheKey;

        public BenefitService(IBenefitRepository benefitRepository, ICacheService cache, IUnitOfWork unitOfWork)
        {
            _benefitRepository = benefitRepository;
            _cache = cache;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Benefit>> GetAllBenefitsAsync()
        {
            var benefitsJson = await _cache.GetStringAsync(_cacheKey);

            if (!string.IsNullOrEmpty(benefitsJson))
            {
                return JsonSerializer.Deserialize<IEnumerable<Benefit>>(benefitsJson);
            }

            var benefits = await _benefitRepository.GetAllAsync();
            await _cache.SetStringAsync(_cacheKey, JsonSerializer.Serialize(benefits));

            return benefits;
        }

        public async Task<Benefit> GetBenefitByIdAsync(Guid id)
        {
            var cacheKey = $"{_cacheKey}-{id}";
            var benefitJson = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(benefitJson))
            {
                return JsonSerializer.Deserialize<Benefit>(benefitJson);
            }

            var benefit = await _benefitRepository.GetByIdAsync(id);
            if (benefit != null)
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(benefit));
            }

            return benefit;
        }

        public async Task<ResponseMessage<Benefit>> AddBenefitAsync(Benefit benefit)
        {
            var existingBenefit = await _benefitRepository.List<Benefit>(b => b.Name.ToLower() == benefit.Name.ToLower());

            if (existingBenefit.Any())
            {
                return ResponseMessage<Benefit>.FailureResponse(ResponseMessageConstants.BenefitAlreadyExists);
            }

            await _benefitRepository.AddAsync(benefit);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync(_cacheKey);
            return ResponseMessage<Benefit>.SuccessResponse(benefit);
        }

        public async Task<ResponseMessage<Benefit>> UpdateBenefitAsync(Benefit benefit)
        {
            var existingBenefit = await _benefitRepository.List<Benefit>(b => b.Name.ToLower() == benefit.Name.ToLower() && b.Id != benefit.Id);

            if (existingBenefit.Any())
            {
                return ResponseMessage<Benefit>.FailureResponse(ResponseMessageConstants.DuplicateBenefit);
            }

            await _benefitRepository.UpdateAsync(benefit);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync(_cacheKey);
            await _cache.RemoveAsync($"{_cacheKey}-{benefit.Id}");
            return ResponseMessage<Benefit>.SuccessResponse(benefit);
        }

        public async Task DeleteBenefitAsync(Guid id)
        {
            await _benefitRepository.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync(_cacheKey);
            await _cache.RemoveAsync($"{_cacheKey}-{id}");
        }
    }
}