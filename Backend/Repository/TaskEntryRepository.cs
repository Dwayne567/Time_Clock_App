using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Interfaces;
using Timeclock_WebApplication.Models;

namespace Timeclock_WebApplication.Repository
{
    // Repository for TaskEntry
    public class TaskEntryRepository : ITaskEntryRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskEntryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all task entries
        public async Task<IEnumerable<TaskEntry>> GetAllAsync()
        {
            return await _context.TaskEntries.ToListAsync();
        }

        // Get task entries by week of date
        public async Task<IEnumerable<TaskEntry>> GetByWeekOfAsync(DateTime weekOf)
        {
            return await _context.TaskEntries
                                 .Where(de => de.WeekOf.HasValue && de.WeekOf.Value.Date == weekOf.Date)
                                 .ToListAsync();
        }

        // Fetch task entries by user and week of date
        public async Task<IEnumerable<TaskEntry>> FetchTaskEntriesByUser(string userId, DateTime weekOf)
        {
            return await _context.TaskEntries
                                .Where(de => de.AppUserId == userId && de.WeekOf.HasValue && de.WeekOf.Value.Date == weekOf.Date)
                                .ToListAsync();
        }

        // Get the last task entry by user
        public async Task<TaskEntry> GetLastTaskEntryByUserAsync(string userId)
        {
            return await _context.TaskEntries
                .Where(te => te.AppUserId == userId)
                .OrderByDescending(te => te.Date)
                .FirstOrDefaultAsync();
        }

        // Get task entry by ID
        public async Task<TaskEntry> GetByIdAsync(int id)
        {
            return await _context.TaskEntries.FindAsync(id);
        }

        // Create a new task entry
        public async Task<TaskEntry> CreateAsync(TaskEntry taskEntry)
        {
            _context.TaskEntries.Add(taskEntry);
            await _context.SaveChangesAsync();
            return taskEntry;
        }

        // Update an existing task entry
        public async Task UpdateAsync(TaskEntry taskEntry)
        {
            _context.Entry(taskEntry).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Delete a task entry by ID
        public async Task DeleteAsync(int id)
        {
            var taskEntry = await _context.TaskEntries.FindAsync(id);
            if (taskEntry != null)
            {
                _context.TaskEntries.Remove(taskEntry);
                await _context.SaveChangesAsync();
            }
        }

        // Delete task entries by date and user ID
        public async Task DeleteByDateAndUserIdAsync(DateTime date, string userId)
        {
            var entriesToDelete = await _context.TaskEntries
                                                .Where(te => te.AppUserId == userId && te.Date.HasValue && te.Date.Value.Date == date.Date)
                                                .ToListAsync();

            _context.TaskEntries.RemoveRange(entriesToDelete);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TaskEntry>> FetchByGroupAndDateRangeAsync(string group, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.TaskEntries
                .Include(te => te.AppUser)
                .Include(te => te.Job)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(group))
            {
                query = query.Where(te => te.AppUser != null && te.AppUser.Group == group);
            }

            if (fromDate.HasValue)
            {
                var start = fromDate.Value.Date;
                query = query.Where(te => te.Date.HasValue && te.Date.Value.Date >= start);
            }

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date.AddDays(1);
                query = query.Where(te => te.Date.HasValue && te.Date.Value < end);
            }

            return await query.OrderBy(te => te.Date).ThenBy(te => te.AppUser!.LastName).ToListAsync();
        }
    }
}
