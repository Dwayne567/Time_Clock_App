using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Interfaces;
using Timeclock_WebApplication.Models;

namespace Timeclock_WebApplication.Repository
{
    // Repository for tasks
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all tasks
        public async Task<IEnumerable<TaskItem>> GetAll()
        {
            return await _context.Tasks.ToListAsync();
        }

        // Get task by ID
        public async Task<TaskItem> GetByIdAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        // Add a new task
        public bool Add(TaskItem task)
        {
            _context.Add(task);
            return Save();
        }

        // Update an existing task
        public bool Update(TaskItem task)
        {
            _context.Update(task);
            return Save();
        }

        // Delete a task
        public bool Delete(TaskItem task)
        {
            _context.Remove(task);
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
