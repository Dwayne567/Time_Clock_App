using System.ComponentModel.DataAnnotations;

namespace FT_TTMS_WebApplication.Models
{
    // Model for created jobs
    public class CreatedJob
    {
        [Key]
        public int Id { get; set; }

        public string? JobNumber { get; set; }
        public string? JobName { get; set; }
        public string? JobNumberAndJobName { get; set; }
    }
}