using TalentSpot.Application.DTOs;
using TalentSpot.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace TalentSpot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        // GET: api/job
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobDTO>>> GetJobs()
        {
            var jobs = await _jobService.GetAllJobAsync();
            return Ok(jobs);
        }

        // GET: api/job/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<JobDTO>> GetJob(Guid id)
        {
            var job = await _jobService.GetJobAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            return Ok(job);
        }

        // POST: api/job
        [HttpPost]
        public async Task<ActionResult<JobDTO>> CreateJob([FromBody] JobDTO jobDTO)
        {
            if (jobDTO == null)
            {
                return BadRequest("Job data is null.");
            }

            var createdJob = await _jobService.CreateJobAsync(jobDTO);
            return CreatedAtAction(nameof(GetJob), new { id = createdJob.Id }, createdJob);
        }

        // PUT: api/job/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateJob(Guid id, [FromBody] JobDTO jobDTO)
        {
            if (id != jobDTO.Id)
            {
                return BadRequest("Job ID mismatch.");
            }

            var result = await _jobService.UpdateJobAsync(jobDTO);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/job/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteJob(Guid id)
        {
            var result = await _jobService.DeleteJobAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // GET: api/job/search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<JobDTO>>> SearchJobs([FromQuery] DateTime expirationDate)
        {
            var jobs = await _jobService.SearchJobsByExpirationDateAsync(expirationDate);
            return Ok(jobs);
        }
    }
}