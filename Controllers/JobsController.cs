using Microsoft.AspNetCore.Mvc;
using FT_TTMS_WebApplication.Models;
using System.Linq;
using FT_TTMS_WebApplication.Data;
using Microsoft.EntityFrameworkCore;

namespace FT_TTMS_WebApplication.Controllers
{
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Displays a list of jobs with optional search and pagination
        public IActionResult Index(string searchTerm, int pageNumber = 1, int pageSize = 100)
        {
            var jobsQuery = _context.Jobs.AsQueryable();

            // Filter jobs based on the search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                jobsQuery = jobsQuery.Where(j => j.JobName.Contains(searchTerm) || j.JobNumber.Contains(searchTerm));
            }

            // Paginate the jobs
            var jobs = jobsQuery
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            var totalJobs = jobsQuery.Count();
            var totalPages = (int)Math.Ceiling(totalJobs / (double)pageSize);

            // Set view bag properties for pagination and search term
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm;

            return View(jobs);
        }

        // Displays the edit form for a job
        public IActionResult Edit(int id)
        {
            var job = _context.Jobs.Find(id);
            if (job == null)
            {
                return NotFound();
            }
            return View(job);
        }

        // Handles the edit form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Job job)
        {
            if (id != job.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _context.Update(job);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(job);
        }

        // Displays the delete confirmation form for a job
        public IActionResult Delete(int id)
        {
            var job = _context.Jobs.Find(id);
            if (job == null)
            {
                return NotFound();
            }
            return View(job);
        }

        // Handles the delete confirmation form submission
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var job = _context.Jobs.Find(id);
            if (job == null)
            {
                return NotFound();
            }

            _context.Jobs.Remove(job);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Displays the job details with associated time entries
        public async Task<IActionResult> JobDetails(string searchTerm)
        {
            var timeEntries = _context.TimeEntries
                .Include(te => te.AppUser)
                .Include(te => te.Job)
                .Where(te => te.Job.JobNumber == searchTerm)
                .Select(te => new
                {
                    te.AppUser.FirstName,
                    te.AppUser.LastName,
                    te.Date,
                    te.Duty,
                    te.Duration
                });

            return View(await timeEntries.ToListAsync());
        }
    }
}