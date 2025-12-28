using Timeclock_WebApplication.Models;

namespace Timeclock_WebApplication.Interfaces
{
    // Interface for Task repository
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetAll();

        Task<TaskItem> GetByIdAsync(int id);

        bool Add(TaskItem task);

        bool Update(TaskItem task);

        bool Delete(TaskItem task);

        bool Save();
    }
}
