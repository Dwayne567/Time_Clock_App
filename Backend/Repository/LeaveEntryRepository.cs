using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Interfaces;
using Timeclock_WebApplication.Models;

namespace Timeclock_WebApplication.Repository
{
    // Repository for LeaveEntry
    public class LeaveEntryRepository : ILeaveEntryRepository
    {
        private readonly ApplicationDbContext _context;

        public LeaveEntryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all leave entries
        public async Task<IEnumerable<LeaveEntry>> GetAllAsync()
        {
            return await _context.LeaveEntries.ToListAsync();
        }

        // Get leave entries by week of date
        public async Task<IEnumerable<LeaveEntry>> GetByWeekOfAsync(DateTime weekOf)
        {
            return await _context.LeaveEntries
                                 .Where(le => le.WeekOf.HasValue && le.WeekOf.Value.Date == weekOf.Date)
                                 .ToListAsync();
        }

        // Fetch leave entries by user and week of date
        public async Task<IEnumerable<LeaveEntry>> FetchLeaveEntriesByUser(string userId, DateTime weekOf)
        {
            return await _context.LeaveEntries
                                .Where(le => le.AppUserId == userId && le.WeekOf.HasValue && le.WeekOf.Value.Date == weekOf.Date)
                                .ToListAsync();
        }

        // Get leave entry by ID
        public async Task<LeaveEntry> GetByIdAsync(int id)
        {
            return await _context.LeaveEntries.FindAsync(id);
        }

        // Create a new leave entry
        public async Task<LeaveEntry> CreateAsync(LeaveEntry leaveEntry)
        {
            _context.LeaveEntries.Add(leaveEntry);
            await _context.SaveChangesAsync();
            return leaveEntry;
        }

        // Update an existing leave entry
        public async Task UpdateAsync(LeaveEntry leaveEntry)
        {
            _context.Entry(leaveEntry).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Delete a leave entry by ID
        public async Task DeleteAsync(int id)
        {
            var leaveEntry = await _context.LeaveEntries.FindAsync(id);
            if (leaveEntry != null)
            {
                _context.LeaveEntries.Remove(leaveEntry);
                await _context.SaveChangesAsync();
            }
        }
    }
}
