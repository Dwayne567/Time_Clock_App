using Microsoft.EntityFrameworkCore;
using FT_TTMS_WebApplication.Data;
using FT_TTMS_WebApplication.Interfaces;
using FT_TTMS_WebApplication.Models;

namespace FT_TTMS_WebApplication.Repository
{
    // Repository for CreatedJob
    public class CreatedJobRepository : ICreatedJobRepository
    {
        private readonly ApplicationDbContext _context;

        public CreatedJobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all created jobs
        public async Task<IEnumerable<CreatedJob>> GetAll()
        {
            return await _context.CreatedJobs.ToListAsync();
        }

        // Create a new created job
        public async Task<CreatedJob> CreateAsync(CreatedJob createdJob)
        {
            _context.CreatedJobs.Add(createdJob);
            await _context.SaveChangesAsync();
            return createdJob;
        }

        // Add a created job
        public bool Add(CreatedJob createdJob)
        {
            _context.Add(createdJob);
            return Save();
        }

        // Update a created job
        public bool Update(CreatedJob createdJob)
        {
            _context.Update(createdJob);
            return Save();
        }

        // Delete a created job
        public bool Delete(CreatedJob createdJob)
        {
            _context.Remove(createdJob);
            return Save();
        }

        // Save changes to the database
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0;
        }

        // Check if a job exists by job number
        public async Task<bool> JobExists(string jobNumber)
        {
            var exists = await _context.CreatedJobs
                .AnyAsync(j => j.JobNumber == jobNumber);

            Console.WriteLine($"Job number {jobNumber} exists: {exists}");
            return exists;
        }
    }
}