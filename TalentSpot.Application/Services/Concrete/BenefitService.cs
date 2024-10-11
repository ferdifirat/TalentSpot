﻿using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;

namespace TalentSpot.Application.Services.Concrete
{
    public class BenefitService : IBenefitService
    {
        private readonly IBenefitRepository _benefitRepository;
        private readonly IDistributedCache _cache;
        private readonly IUnitOfWork _unitOfWork;

        public BenefitService(IBenefitRepository benefitRepository, IDistributedCache cache, IUnitOfWork unitOfWork)
        {
            _benefitRepository = benefitRepository;
            _cache = cache;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Benefit>> GetAllBenefitsAsync()
        {
            var cacheKey = "benefits";
            var benefitsJson = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(benefitsJson))
            {
                return JsonSerializer.Deserialize<IEnumerable<Benefit>>(benefitsJson);
            }

            var benefits = await _benefitRepository.GetAllAsync();
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(benefits));

            return benefits;
        }

        public async Task<Benefit> GetBenefitByIdAsync(Guid id)
        {
            var cacheKey = $"benefit-{id}";
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

        public async Task AddBenefitAsync(Benefit benefit)
        {
            await _benefitRepository.AddAsync(benefit);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync("benefits");
        }

        public async Task UpdateBenefitAsync(Benefit benefit)
        {
            await _benefitRepository.UpdateAsync(benefit);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync("benefits");
            await _cache.RemoveAsync($"benefit-{benefit.Id}");
        }

        public async Task DeleteBenefitAsync(Guid id)
        {
            await _benefitRepository.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync("benefits");
            await _cache.RemoveAsync($"benefit-{id}");
        }
    }
}
