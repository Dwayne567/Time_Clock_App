using Timeclock_WebApplication.Models;

namespace Timeclock_WebApplication.Interfaces
{
    // Interface for User repository
    public interface IUserRepository
    {
        bool Add(AppUser user);
        bool Save();
        Task<IEnumerable<AppUser>> GetAllUsers();
        Task<AppUser?> GetUserById(string id);
        Task<IEnumerable<AppUser>> GetUsersByGroup(string group);
        bool Update(AppUser user);
        bool Delete(AppUser user);
    }
}
