using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentSpot.Application.Services;
using TalentSpot.Domain.Entities;

namespace TalentSpot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ForbiddenWordController : ControllerBase
    {
        private readonly IForbiddenWordService _forbiddenWordService;

        public ForbiddenWordController(IForbiddenWordService forbiddenWordService)
        {
            _forbiddenWordService = forbiddenWordService;
        }

        [HttpGet]
        public async Task<IActionResult> GetForbiddenWords()
        {
            var forbiddenWords = await _forbiddenWordService.GetAllForbiddenWordsAsync();
            return Ok(forbiddenWords);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetForbiddenWord(Guid id)
        {
            var forbiddenWord = await _forbiddenWordService.GetForbiddenWordByIdAsync(id);
            if (forbiddenWord == null)
            {
                return NotFound();
            }
            return Ok(forbiddenWord);
        }

        [HttpPost]
        public async Task<IActionResult> AddForbiddenWord([FromBody] ForbiddenWord forbiddenWord)
        {
            await _forbiddenWordService.AddForbiddenWordAsync(forbiddenWord);
            return CreatedAtAction(nameof(GetForbiddenWord), new { id = forbiddenWord.Id }, forbiddenWord);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateForbiddenWord(Guid id, [FromBody] ForbiddenWord forbiddenWord)
        {
            if (id != forbiddenWord.Id)
            {
                return BadRequest();
            }
            await _forbiddenWordService.UpdateForbiddenWordAsync(forbiddenWord);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteForbiddenWord(Guid id)
        {
            await _forbiddenWordService.DeleteForbiddenWordAsync(id);
            return NoContent();
        }
    }
}