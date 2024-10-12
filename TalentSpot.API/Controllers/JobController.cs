using TalentSpot.Application.DTOs;
using TalentSpot.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TalentSpot.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobDTO>>> GetAllJobs()
        {
            var jobs = await _jobService.GetAllJobAsync();

            if (!jobs.Success)
            {
                return NotFound(jobs);
            }

            return Ok(jobs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobDTO>> GetJob(Guid id)
        {
            var job = await _jobService.GetJobAsync(id);

            if (!job.Success)
            {
                return NotFound(job);
            }

            return Ok(job);
        }

        [HttpPost]
        public async Task<ActionResult<JobDTO>> CreateJob([FromBody] JobCreateDTO jobCreateDTO)
        {
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var createdJob = await _jobService.CreateJobAsync(jobCreateDTO, userId);

            if (!createdJob.Success)
            {
                return BadRequest(createdJob);
            }

            return Ok(createdJob);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateJob([FromBody] JobUpdateDTO jobUpdateDTO)
        {
            var result = await _jobService.UpdateJobAsync(jobUpdateDTO);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteJob(Guid id)
        {
            var result = await _jobService.DeleteJobAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return Ok(id);
        }

        [HttpGet("search")]
        public async Task<ActionResult<ResponseMessage<List<JobDTO>>>> SearchJobs([FromQuery] string startDate, [FromQuery] string endDate)
        {

            if (!DateTime.TryParse(startDate, out var parsedStartDate) || !DateTime.TryParse(endDate, out var parsedEndDate))
            {
                return BadRequest("Geçersiz tarih formatý.");
            }

            var jobsResponse = await _jobService.SearchJobsByExpirationDateRangeAsync(parsedStartDate.ToUniversalTime(), parsedEndDate.ToUniversalTime());

            // Eðer baþarýlý deðilse 404 NotFound dönüyoruz.
            if (!jobsResponse.Success)
            {
                return NotFound(jobsResponse);
            }

            // Baþarýlý ise 200 OK dönüyoruz.
            return Ok(jobsResponse);
        }
    }
}