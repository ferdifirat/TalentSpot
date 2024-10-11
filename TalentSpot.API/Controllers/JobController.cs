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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobDTO>>> GetJobs()
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
            var createdJob = await _jobService.CreateJobAsync(jobCreateDTO);

            if (!createdJob.Success)
            {
                return BadRequest(createdJob);
            }

            return Ok(createdJob);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateJob(Guid id, [FromBody] JobDTO jobDTO)
        {
            var result = await _jobService.UpdateJobAsync(jobDTO);
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
        public async Task<ActionResult<IEnumerable<JobDTO>>> SearchJobs([FromQuery] DateTime expirationDate)
        {
            var jobs = await _jobService.SearchJobsByExpirationDateAsync(expirationDate);

            if (!jobs.Success)
            {
                return NotFound(jobs);
            }

            return Ok(jobs);
        }
    }
}