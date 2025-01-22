using Microsoft.EntityFrameworkCore;
using FT_TTMS_WebApplication.Data;
using FT_TTMS_WebApplication.Interfaces;
using FT_TTMS_WebApplication.Models;

namespace FT_TTMS_WebApplication.Repository
{
    // Repository for Duty
    public class DutyRepository : IDutyRepository
    {
        private readonly ApplicationDbContext _context;

        public DutyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all duties
        public async Task<IEnumerable<Duty>> GetAll()
        {
            return await _context.Duties.ToListAsync();
        }

        // Get duty by ID
        public async Task<Duty> GetByIdAsync(int id)
        {
            return await _context.Duties.FindAsync(id);
        }

        // Add a new duty
        public bool Add(Duty duty)
        {
            _context.Add(duty);
            return Save();
        }

        // Update an existing duty
        public bool Update(Duty duty)
        {
            _context.Update(duty);
            return Save();
        }

        // Delete a duty
        public bool Delete(Duty duty)
        {
            _context.Remove(duty);
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