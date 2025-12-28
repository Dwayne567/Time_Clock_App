using Microsoft.EntityFrameworkCore;
using Timeclock_WebApplication.Data;
using Timeclock_WebApplication.Interfaces;
using Timeclock_WebApplication.Models;

namespace Timeclock_WebApplication.Repository
{
    // Repository for User
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add a new user
        public bool Add(AppUser user)
        {
            _context.Users.Add(user);
            return Save();
        }

        // Save changes to the database
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0;
        }

        // Get all users
        public async Task<IEnumerable<AppUser>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // Get user by ID
        public async Task<AppUser?> GetUserById(string id)
        {
            return await _context.Users.FindAsync(id);
        }

        // Get users by group
        public async Task<IEnumerable<AppUser>> GetUsersByGroup(string group)
        {
            return await _context.Users
                .Include(u => u.TaskEntries)
                .Where(u => u.Group == group)
                .ToListAsync();
        }

        // Update an existing user
        public bool Update(AppUser user)
        {
            _context.Update(user);
            return Save();
        }

        // Delete a user
        public bool Delete(AppUser user)
        {
            var existingUser = _context.Users.Find(user.Id);
            if (existingUser == null) return false;
            _context.Users.Remove(existingUser);
            return Save();
        }
    }
}
