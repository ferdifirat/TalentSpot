using System.Text.Json;
using TalentSpot.Application.Constants;
using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;
using TalentSpot.Infrastructure.Interfaces;

namespace TalentSpot.Application.Services.Concrete
{
    public class WorkTypeService : IWorkTypeService
    {
        private readonly IWorkTypeRepository _workTypeRepository;
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _cacheKey = RedisCacheConstants.WorkTypeCacheKey;

        public WorkTypeService(IWorkTypeRepository workTypeRepository, ICacheService cache, IUnitOfWork unitOfWork)
        {
            _workTypeRepository = workTypeRepository;
            _cache = cache;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<WorkType>> GetAllWorkTypesAsync()
        {
            var workTypesJson = await _cache.GetStringAsync(_cacheKey);

            if (!string.IsNullOrEmpty(workTypesJson))
            {
                return JsonSerializer.Deserialize<IEnumerable<WorkType>>(workTypesJson);
            }

            var workTypes = await _workTypeRepository.GetAllAsync();
            await _cache.SetStringAsync(_cacheKey, JsonSerializer.Serialize(workTypes));

            return workTypes;
        }

        public async Task<WorkType> GetWorkTypeByIdAsync(Guid id)
        {
            var cacheKey = $"{_cacheKey}-{id}";
            var workTypeJson = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(workTypeJson))
            {
                return JsonSerializer.Deserialize<WorkType>(workTypeJson);
            }

            var workType = await _workTypeRepository.GetByIdAsync(id);
            if (workType != null)
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(workType));
            }

            return workType;
        }

        public async Task<ResponseMessage<WorkType>> AddWorkTypeAsync(WorkType workType)
        {
            var existingWorkType = await _workTypeRepository.List<WorkType>(wt => wt.Name.ToLower() == workType.Name.ToLower());

            if (existingWorkType.Any())
            {
                return ResponseMessage<WorkType>.FailureResponse(ResponseMessages.WorkTypeAlreadyExists);
            }

            await _workTypeRepository.AddAsync(workType);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync(_cacheKey);
            return ResponseMessage<WorkType>.SuccessResponse(workType);
        }

        public async Task<ResponseMessage<WorkType>> UpdateWorkTypeAsync(WorkType workType)
        {
            var existingWorkType = await _workTypeRepository.List<WorkType>(wt => wt.Name.ToLower() == workType.Name.ToLower() && wt.Id != workType.Id);

            if (existingWorkType.Any())
            {
                return ResponseMessage<WorkType>.FailureResponse(ResponseMessages.WorkTypeExistsElsewhere);
            }

            await _workTypeRepository.UpdateAsync(workType);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync(_cacheKey);
            await _cache.RemoveAsync($"{_cacheKey}-{workType.Id}");
            return ResponseMessage<WorkType>.SuccessResponse(workType);
        }

        public async Task DeleteWorkTypeAsync(Guid id)
        {
            await _workTypeRepository.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync(_cacheKey);
            await _cache.RemoveAsync($"{_cacheKey}-{id}");
        }
    }
}