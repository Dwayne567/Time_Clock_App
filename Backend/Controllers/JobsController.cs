using Microsoft.AspNetCore.Mvc;
using Timeclock_WebApplication.Models;
using System.Linq;
using Timeclock_WebApplication.Data;
using Microsoft.EntityFrameworkCore;

namespace Timeclock_WebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Jobs
        [HttpGet]
        public async Task<IActionResult> GetJobs(string searchTerm = "", int pageNumber = 1, int pageSize = 100)
        {
            var jobsQuery = _context.Jobs.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                jobsQuery = jobsQuery.Where(j => j.JobName.Contains(searchTerm) || j.JobNumber.Contains(searchTerm));
            }

            var totalJobs = await jobsQuery.CountAsync();
            var jobs = await jobsQuery
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

            return Ok(new
            {
                TotalJobs = totalJobs,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalJobs / (double)pageSize),
                Jobs = jobs
            });
        }

        // GET: api/Jobs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return NotFound();
            }

            return Ok(job);
        }

        // PUT: api/Jobs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(int id, Job job)
        {
            if (id != job.Id)
            {
                return BadRequest();
            }

            _context.Entry(job).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Jobs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            // Check if there are any task entries associated with this job
            var hasTaskEntries = await _context.TaskEntries.AnyAsync(te => te.JobId == id);
            if (hasTaskEntries)
            {
                return BadRequest("Cannot delete this job because it has associated task entries. Please delete the task entries first.");
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return Ok("Job deleted successfully.");
        }

        // GET: api/Jobs/Details/{searchTerm}
        [HttpGet("Details/{searchTerm}")]
        public async Task<IActionResult> JobDetails(string searchTerm)
        {
            var taskEntries = await _context.TaskEntries
                .Include(te => te.AppUser)
                .Include(te => te.Job)
                .Where(te => te.Job.JobNumber == searchTerm)
                .Select(te => new
                {
                    te.AppUser.FirstName,
                    te.AppUser.LastName,
                    te.Date,
                    te.TaskName,
                    te.Duration
                })
                .ToListAsync();

            return Ok(taskEntries);
        }

        private bool JobExists(int id)
        {
            return _context.Jobs.Any(e => e.Id == id);
        }
    }
}
