using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Interfaces;
using Timeclock_WebApplication.Models;

namespace Timeclock_WebApplication.Repository
{
    // Repository for Job
    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _context;

        public JobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all jobs
        public async Task<IEnumerable<Job>> GetAll()
        {
            return await _context.Jobs.ToListAsync();
        }

        // Create a new job
        public async Task<Job> CreateAsync(Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        // Add a new job
        public bool Add(Job job)
        {
            _context.Add(job);
            return Save();
        }

        // Update an existing job
        public bool Update(Job job)
        {
            _context.Update(job);
            return Save();
        }

        // Delete a job
        public bool Delete(Job job)
        {
            _context.Remove(job);
            return Save();
        }

        // Save changes to the database
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0;
        }

        // Find a job by job number
        public async Task<Job> FindByJobNumberAsync(string jobNumber)
        {
            return await _context.Jobs.FirstOrDefaultAsync(j => j.JobNumber == jobNumber);
        }
    }
}
