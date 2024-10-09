using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace TalentSpot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForbiddenWordsController : ControllerBase
    {
        private readonly IDistributedCache _cache;
        private const string ForbiddenWordsCacheKey = "ForbiddenWords";

        public ForbiddenWordsController(IDistributedCache cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public async Task<IActionResult> GetForbiddenWords()
        {
            var forbiddenWords = await GetForbiddenWordsFromCache();
            return Ok(forbiddenWords);
        }

        [HttpPost]
        public async Task<IActionResult> AddForbiddenWord([FromBody] string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return BadRequest("Kelime boþ olamaz.");
            }

            var forbiddenWords = await GetForbiddenWordsFromCache();
            if (!forbiddenWords.Contains(word))
            {
                forbiddenWords.Add(word);
                await SetForbiddenWordsToCache(forbiddenWords);
            }

            return CreatedAtAction(nameof(GetForbiddenWords), new { word });
        }

        [HttpDelete("{word}")]
        public async Task<IActionResult> DeleteForbiddenWord(string word)
        {
            var forbiddenWords = await GetForbiddenWordsFromCache();
            if (forbiddenWords.Contains(word))
            {
                forbiddenWords.Remove(word);
                await SetForbiddenWordsToCache(forbiddenWords);
                return NoContent();
            }

            return NotFound("Yasaklý kelime bulunamadý.");
        }

        private async Task<List<string>> GetForbiddenWordsFromCache()
        {
            var cachedData = await _cache.GetStringAsync(ForbiddenWordsCacheKey);
            return string.IsNullOrEmpty(cachedData) ? new List<string>() : Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(cachedData);
        }

        private async Task SetForbiddenWordsToCache(List<string> forbiddenWords)
        {
            var serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(forbiddenWords);
            await _cache.SetStringAsync(ForbiddenWordsCacheKey, serializedData);
        }
    }
}