using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Timeclock_WebApplication.Models
{
    // Model for jobs
    public class Job
    {
        [Key]
        public int Id { get; set; }

        public string? JobNumber { get; set; }
        public string? JobName { get; set; }
        public string? JobNumberAndJobName { get; set; }
    }
}
