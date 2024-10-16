using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentSpot.Application.DTOs;
using TalentSpot.Application.Services;

namespace TalentSpot.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobDTO>>> GetAllJobs()
        {
            var jobs = await _companyService.GetAllCompanyAsync();

            if (!jobs.Success)
            {
                return NotFound(jobs);
            }

            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompany(Guid id)
        {
            var company = await _companyService.GetCompanyAsync(id);
            return Ok(company);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCompany([FromBody] CompanyDTO companyDTO)
        {
            var updated = await _companyService.UpdateCompanyAsync(companyDTO);
            return Ok(updated);
        }
    }
}