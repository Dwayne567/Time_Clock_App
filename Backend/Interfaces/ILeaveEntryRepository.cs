using Timeclock_WebApplication.Models;

namespace Timeclock_WebApplication.Interfaces
{
    // Interface for LeaveEntry repository
    public interface ILeaveEntryRepository
    {
        Task<IEnumerable<LeaveEntry>> GetAllAsync();
        Task<IEnumerable<LeaveEntry>> GetByWeekOfAsync(DateTime weekOf);
        Task<IEnumerable<LeaveEntry>> FetchLeaveEntriesByUser(string userId, DateTime weekOf);
        Task<LeaveEntry> GetByIdAsync(int id);
        Task<LeaveEntry> CreateAsync(LeaveEntry leaveEntry);
        Task UpdateAsync(LeaveEntry leaveEntry);
        Task DeleteAsync(int id);
    }
}
