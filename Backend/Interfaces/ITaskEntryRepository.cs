using Timeclock_WebApplication.Models;

namespace Timeclock_WebApplication.Interfaces
{
    // Interface for TaskEntry repository
    public interface ITaskEntryRepository
    {
        Task<IEnumerable<TaskEntry>> GetAllAsync();
        Task<IEnumerable<TaskEntry>> GetByWeekOfAsync(DateTime weekOf);
        Task<IEnumerable<TaskEntry>> FetchTaskEntriesByUser(string userId, DateTime weekOf);
        Task<TaskEntry> GetLastTaskEntryByUserAsync(string userId);
        Task<TaskEntry> GetByIdAsync(int id);
        Task<TaskEntry> CreateAsync(TaskEntry taskEntry);
        Task UpdateAsync(TaskEntry taskEntry);
        Task DeleteAsync(int id);
        Task DeleteByDateAndUserIdAsync(DateTime date, string userId);
        Task<IEnumerable<TaskEntry>> FetchByGroupAndDateRangeAsync(string group, DateTime? fromDate, DateTime? toDate);
    }
}
