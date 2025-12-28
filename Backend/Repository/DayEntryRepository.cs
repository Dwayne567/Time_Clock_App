using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Interfaces;
using Timeclock_WebApplication.Models;

namespace Timeclock_WebApplication.Repository
{
    // Repository for DayEntry
    public class DayEntryRepository : IDayEntryRepository
    {
        private readonly ApplicationDbContext _context;

        public DayEntryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all day entries
        public async Task<IEnumerable<DayEntry>> GetAllAsync()
        {
            return await _context.DayEntries.ToListAsync();
        }

        // Get day entries by week of date
        public async Task<IEnumerable<DayEntry>> GetByWeekOfAsync(DateTime weekOf)
        {
            return await _context.DayEntries
                                 .Where(de => de.WeekOf.HasValue && de.WeekOf.Value.Date == weekOf.Date)
                                 .ToListAsync();
        }

        // Fetch day entries by user and week of date
        public async Task<IEnumerable<DayEntry>> fetchDayEntriesByUser(string userId, DateTime weekOf)
        {
            return await _context.DayEntries
                                .Where(de => de.AppUserId == userId && de.WeekOf.HasValue && de.WeekOf.Value.Date == weekOf.Date)
                                .ToListAsync();
        }

        // Get day entry by ID
        public async Task<DayEntry> GetByIdAsync(int id)
        {
            return await _context.DayEntries.FindAsync(id);
        }

        // Create a new day entry
        public async Task<DayEntry> CreateAsync(DayEntry dayEntry)
        {
            _context.DayEntries.Add(dayEntry);
            await _context.SaveChangesAsync();
            return dayEntry;
        }

        // Update an existing day entry
        public async Task UpdateAsync(DayEntry dayEntry)
        {
            _context.Entry(dayEntry).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Delete a day entry by ID
        public async Task DeleteAsync(int id)
        {
            var dayEntry = await _context.DayEntries.FindAsync(id);
            if (dayEntry != null)
            {
                _context.DayEntries.Remove(dayEntry);
                await _context.SaveChangesAsync();
            }
        }

        // Delete day entries by date and user ID
        public async Task DeleteByDateAndUserIdAsync(DateTime date, string userId)
        {
            var entriesToDelete = await _context.DayEntries
                                                .Where(te => te.AppUserId == userId && te.Date.HasValue && te.Date.Value.Date == date.Date)
                                                .ToListAsync();

            _context.DayEntries.RemoveRange(entriesToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
