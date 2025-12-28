using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeclock_WebApplication.Models
{
    // Model for day entries
    public class DayEntry
    {
        [Key]
        public int Id { get; set; }
        public string? AppUserId { get; set; }

        // Navigation property
        [ForeignKey("AppUserId")]
        public AppUser? AppUser { get; set; }

        [DataType(DataType.Date)]
        public DateTime? WeekOf { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }
        public string? DayName { get; set; }
        public TimeSpan? DayStartTime { get; set; }
        public TimeSpan? DayEndTime { get; set; }
        public TimeSpan? LunchStartTime { get; set; }
        public TimeSpan? LunchEndTime { get; set; }

        public double? DayDuration { get; set; }
        public double? LunchDuration { get; set; }
        public double? WorkDuration { get; set; }

        // Comment
        public string? Comment { get; set; }

        // Status
        public string? Status { get; set; }
    }
}
