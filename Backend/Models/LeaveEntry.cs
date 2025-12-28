using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeclock_WebApplication.Models
{
    // Model for leave entries
    public class LeaveEntry
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

        public string? LeaveType { get; set; }

        public double? LeaveDuration { get; set; }

        // Status
        public string? Status { get; set; }
    }
}
