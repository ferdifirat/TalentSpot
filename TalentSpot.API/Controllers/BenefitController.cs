using TalentSpot.Application.Services;
using Microsoft.AspNetCore.Mvc;
using TalentSpot.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace TalentSpot.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class BenefitController : ControllerBase
    {
        private readonly IBenefitService _benefitService;

        public BenefitController(IBenefitService benefitService)
        {
            _benefitService = benefitService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBenefits()
        {
            var benefits = await _benefitService.GetAllBenefitsAsync();
            return Ok(benefits);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBenefit(Guid id)
        {
            var benefit = await _benefitService.GetBenefitByIdAsync(id);
            if (benefit == null)
            {
                return NotFound();
            }
            return Ok(benefit);
        }

        [HttpPost]
        public async Task<IActionResult> AddBenefit([FromBody] Benefit benefit)
        {
            await _benefitService.AddBenefitAsync(benefit);
            return CreatedAtAction(nameof(GetBenefit), new { id = benefit.Id }, benefit);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBenefit(Guid id, [FromBody] Benefit benefit)
        {
            if (id != benefit.Id)
            {
                return BadRequest();
            }
            await _benefitService.UpdateBenefitAsync(benefit);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBenefit(Guid id)
        {
            await _benefitService.DeleteBenefitAsync(id);
            return Ok();
        }
    }
}