using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalentSpot.Application.Services.Concrete
{
    public class ForbiddenWordService : IForbiddenWordService
    {
        private readonly IDistributedCache _cache;
        private const string ForbiddenWordsKey = "ForbiddenWords";

        public ForbiddenWordService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetForbiddenWordsAsync(IEnumerable<string> words)
        {
            // Store the forbidden words as a comma-separated string in Redis
            var forbiddenWords = string.Join(",", words);
            await _cache.SetStringAsync(ForbiddenWordsKey, forbiddenWords);
        }

        public async Task<List<string>> GetForbiddenWordsAsync()
        {
            var forbiddenWords = await _cache.GetStringAsync(ForbiddenWordsKey);
            return string.IsNullOrEmpty(forbiddenWords)
                ? new List<string>()
                : forbiddenWords.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
