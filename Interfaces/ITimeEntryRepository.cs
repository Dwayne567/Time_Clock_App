using FT_TTMS_WebApplication.Models;

namespace FT_TTMS_WebApplication.Interfaces
{
    // Interface for TimeEntry repository
    public interface ITimeEntryRepository
    {
        Task<IEnumerable<TimeEntry>> GetAllAsync();
        Task<IEnumerable<TimeEntry>> GetByWeekOfAsync(DateTime weekOf);
        Task<IEnumerable<TimeEntry>> fetchTimeEntriesByUser(string userId, DateTime weekOf);
        Task<TimeEntry> GetLastTimeEntryByUserAsync(string userId);
        Task<TimeEntry> GetByIdAsync(int id);
        Task<TimeEntry> CreateAsync(TimeEntry timeEntry);
        Task UpdateAsync(TimeEntry timeEntry);
        Task DeleteAsync(int id);
        Task DeleteByDateAndUserIdAsync(DateTime date, string userId);
    }
}