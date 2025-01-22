using Microsoft.EntityFrameworkCore;
using FT_TTMS_WebApplication.Data;
using FT_TTMS_WebApplication.Interfaces;
using FT_TTMS_WebApplication.Models;

namespace FT_TTMS_WebApplication.Repository
{
    // Repository for TimeEntry
    public class TimeEntryRepository : ITimeEntryRepository
    {
        private readonly ApplicationDbContext _context;

        public TimeEntryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all time entries
        public async Task<IEnumerable<TimeEntry>> GetAllAsync()
        {
            return await _context.TimeEntries.ToListAsync();
        }

        // Get time entries by week of date
        public async Task<IEnumerable<TimeEntry>> GetByWeekOfAsync(DateTime weekOf)
        {
            return await _context.TimeEntries
                                 .Where(de => de.WeekOf.HasValue && de.WeekOf.Value.Date == weekOf.Date)
                                 .ToListAsync();
        }

        // Fetch time entries by user and week of date
        public async Task<IEnumerable<TimeEntry>> fetchTimeEntriesByUser(string userId, DateTime weekOf)
        {
            return await _context.TimeEntries
                                .Where(de => de.AppUserId == userId && de.WeekOf.HasValue && de.WeekOf.Value.Date == weekOf.Date)
                                .ToListAsync();
        }

        // Get the last time entry by user
        public async Task<TimeEntry> GetLastTimeEntryByUserAsync(string userId)
        {
            return await _context.TimeEntries
                .Where(te => te.AppUserId == userId)
                .OrderByDescending(te => te.Date)
                .FirstOrDefaultAsync();
        }

        // Get time entry by ID
        public async Task<TimeEntry> GetByIdAsync(int id)
        {
            return await _context.TimeEntries.FindAsync(id);
        }

        // Create a new time entry
        public async Task<TimeEntry> CreateAsync(TimeEntry timeEntry)
        {
            _context.TimeEntries.Add(timeEntry);
            await _context.SaveChangesAsync();
            return timeEntry;
        }

        // Update an existing time entry
        public async Task UpdateAsync(TimeEntry timeEntry)
        {
            _context.Entry(timeEntry).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Delete a time entry by ID
        public async Task DeleteAsync(int id)
        {
            var timeEntry = await _context.TimeEntries.FindAsync(id);
            if (timeEntry != null)
            {
                _context.TimeEntries.Remove(timeEntry);
                await _context.SaveChangesAsync();
            }
        }

        // Delete time entries by date and user ID
        public async Task DeleteByDateAndUserIdAsync(DateTime date, string userId)
        {
            var entriesToDelete = await _context.TimeEntries
                                                .Where(te => te.AppUserId == userId && te.Date.HasValue && te.Date.Value.Date == date.Date)
                                                .ToListAsync();

            _context.TimeEntries.RemoveRange(entriesToDelete);
            await _context.SaveChangesAsync();
        }
    }
}