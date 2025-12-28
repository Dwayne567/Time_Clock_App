using Timeclock_WebApplication.Models;

namespace Timeclock_WebApplication.Interfaces
{
    // Interface for DayEntry repository
    public interface IDayEntryRepository
    {
        Task<IEnumerable<DayEntry>> GetAllAsync();
        Task<IEnumerable<DayEntry>> GetByWeekOfAsync(DateTime weekOf);
        Task<IEnumerable<DayEntry>> fetchDayEntriesByUser(string userId, DateTime date);
        Task<DayEntry> GetByIdAsync(int id);
        Task<DayEntry> CreateAsync(DayEntry dayEntry);
        Task UpdateAsync(DayEntry dayEntry);
        Task DeleteAsync(int id);
        Task DeleteByDateAndUserIdAsync(DateTime date, string userId);
    }
}
