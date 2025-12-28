using Microsoft.AspNetCore.Identity;

namespace Timeclock_WebApplication.Models
{
    // Application user model extending IdentityUser
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? EmployeeNumber { get; set; }
        public string? Group { get; set; }
        public virtual ICollection<DayEntry>? DayEntries { get; set; }
        public virtual ICollection<TaskEntry>? TaskEntries { get; set; }
    }
}
