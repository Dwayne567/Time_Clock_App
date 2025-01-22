using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FT_TTMS_WebApplication.Models
{
    // Model for time entries
    public class TimeEntry
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

        public int? JobId { get; set; }

        [ForeignKey("JobId")]
        public Job? Job { get; set; }

        public string? Duty { get; set; }

        public double? Duration { get; set; }
        public string? Comment { get; set; }

        // Status
        public string? Status { get; set; }
    }
}