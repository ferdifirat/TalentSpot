using Microsoft.Extensions.Caching.Distributed;

namespace TalentSpot.Infrastructure.Interfaces
{
    public interface ICacheService
    {
        Task<string> GetStringAsync(string key);
        Task SetStringAsync(string key, string value);
        Task RemoveAsync(string key);
        Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options);
    }
}
