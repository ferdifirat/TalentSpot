using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentSpot.Application.Services;
using TalentSpot.Domain.Entities;

namespace TalentSpot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkTypeController : ControllerBase
    {
        private readonly IWorkTypeService _workTypeService;

        public WorkTypeController(IWorkTypeService workTypeService)
        {
            _workTypeService = workTypeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetWorkTypes()
        {
            var workTypes = await _workTypeService.GetAllWorkTypesAsync();
            return Ok(workTypes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkType(Guid id)
        {
            var workType = await _workTypeService.GetWorkTypeByIdAsync(id);
            if (workType == null)
            {
                return NotFound();
            }
            return Ok(workType);
        }

        [HttpPost]
        public async Task<IActionResult> AddWorkType([FromBody] WorkType workType)
        {
            await _workTypeService.AddWorkTypeAsync(workType);
            return CreatedAtAction(nameof(GetWorkType), new { id = workType.Id }, workType);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkType(Guid id, [FromBody] WorkType workType)
        {
            if (id != workType.Id)
            {
                return BadRequest();
            }
            await _workTypeService.UpdateWorkTypeAsync(workType);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkType(Guid id)
        {
            await _workTypeService.DeleteWorkTypeAsync(id);
            return Ok();
        }
    }
}