using Microsoft.EntityFrameworkCore;
using FT_TTMS_WebApplication.Data;
using FT_TTMS_WebApplication.Interfaces;
using FT_TTMS_WebApplication.Models;

namespace FT_TTMS_WebApplication.Repository
{
    // Repository for ImportedJob
    public class ImportedJobRepository : IImportedJobRepository
    {
        private readonly ApplicationDbContext _context;

        public ImportedJobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all imported jobs
        public async Task<IEnumerable<ImportedJob>> GetAll()
        {
            return await _context.ImportedJobs.ToListAsync();
        }

        // Create a new imported job
        public async Task<ImportedJob> CreateAsync(ImportedJob job)
        {
            _context.ImportedJobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        // Add a new imported job
        public bool Add(ImportedJob job)
        {
            _context.Add(job);
            return Save();
        }

        // Update an existing imported job
        public bool Update(ImportedJob job)
        {
            _context.Update(job);
            return Save();
        }

        // Delete an imported job
        public bool Delete(ImportedJob job)
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
    }
}