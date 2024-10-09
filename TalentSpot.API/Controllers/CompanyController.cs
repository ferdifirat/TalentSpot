using TalentSpot.Application.DTOs;
using TalentSpot.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace TalentSpot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCompany([FromBody] CompanyDTO companyDTO)
        {
            try
            {
                var result = await _companyService.RegisterCompanyAsync(companyDTO);
                return CreatedAtAction(nameof(GetCompany), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompany(Guid id)
        {
            try
            {
                var company = await _companyService.GetCompanyAsync(id);
                return Ok(company);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCompany([FromBody] CompanyDTO companyDTO)
        {
            try
            {
                var updated = await _companyService.UpdateCompanyAsync(companyDTO);
                return updated ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}