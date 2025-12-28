using System.ComponentModel.DataAnnotations;

namespace Timeclock_WebApplication.Models
{
    // Model for tasks
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }
        public string TaskDescription { get; set; }
    }
}
