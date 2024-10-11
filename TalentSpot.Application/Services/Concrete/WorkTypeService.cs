using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using TalentSpot.Application.DTOs;
using TalentSpot.Domain.Entities;
using TalentSpot.Domain.Interfaces;
namespace TalentSpot.Application.Services.Concrete
{
    public class WorkTypeService : IWorkTypeService
    {
        private readonly IWorkTypeRepository _workTypeRepository;
        private readonly IDistributedCache _cache;
        private readonly IUnitOfWork _unitOfWork;

        public WorkTypeService(IWorkTypeRepository workTypeRepository, IDistributedCache cache, IUnitOfWork unitOfWork)
        {
            _workTypeRepository = workTypeRepository;
            _cache = cache;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<WorkType>> GetAllWorkTypesAsync()
        {
            var cacheKey = "worktypes";
            var workTypesJson = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(workTypesJson))
            {
                return JsonSerializer.Deserialize<IEnumerable<WorkType>>(workTypesJson);
            }

            var workTypes = await _workTypeRepository.GetAllAsync();
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(workTypes));

            return workTypes;
        }

        public async Task<WorkType> GetWorkTypeByIdAsync(Guid id)
        {
            var cacheKey = $"worktype-{id}";
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
            var existingWorkType = await _workTypeRepository.FindAsync(wt => wt.Name.ToLower() == workType.Name.ToLower());

            if (existingWorkType != null)
            {
                return ResponseMessage<WorkType>.FailureResponse("Çalışma türü zaten mevcut.");
            }

            await _workTypeRepository.AddAsync(workType);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync("worktypes");
            return ResponseMessage<WorkType>.SuccessResponse(workType);
        }

        public async Task<ResponseMessage<WorkType>> UpdateWorkTypeAsync(WorkType workType)
        {
            var existingWorkType = await _workTypeRepository.FindAsync(wt => wt.Name.ToLower() == workType.Name.ToLower() && wt.Id != workType.Id);

            if (existingWorkType != null)
            {
                return ResponseMessage<WorkType>.FailureResponse("Aynı çalışma türü zaten başka bir kayıt olarak mevcut.");
            }

            await _workTypeRepository.UpdateAsync(workType);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync("worktypes");
            await _cache.RemoveAsync($"worktype-{workType.Id}");
            return ResponseMessage<WorkType>.SuccessResponse(workType);
        }

        public async Task DeleteWorkTypeAsync(Guid id)
        {
            await _workTypeRepository.DeleteAsync(id);
            await _unitOfWork.CompleteAsync();
            await _cache.RemoveAsync("worktypes"); 
            await _cache.RemoveAsync($"worktype-{id}");
        }
    }
}