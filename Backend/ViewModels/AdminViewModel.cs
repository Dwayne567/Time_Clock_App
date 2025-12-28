using Timeclock_WebApplication.Models;

namespace Timeclock_WebApplication.ViewModels
{
    // ViewModel for Admin
    public class AdminViewModel
    {
        public int Id { get; set; }
        public string? AppUserId { get; set; }
        public string? Task { get; set; }
        public string? Job { get; set; }
        public DateTime WeekOf { get; set; }
        public DayOfWeek Day { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string? Comment { get; set; }
        public DateTime DateHidden { get; set; }
        public TaskEntry? TaskEntry { get; set; }
        public Dictionary<int, TaskEntry>? TaskEntriesById { get; set; }
        public Dictionary<DayOfWeek, List<TaskEntry>>? TaskEntriesByDay { get; set; }
        public IEnumerable<Job>? Jobs { get; set; }
        public DateTime WeekStart { get; set; }
        public List<TaskEntry>? AllTaskEntries { get; set; }
        public IEnumerable<AppUser> Users { get; set; }
    }
}
