using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FT_TTMS_WebApplication.Models
{
    // Model for imported jobs
    public class ImportedJob
    {
        [Key]
        public int Id { get; set; }

        public string? JobNumber { get; set; }
        public string? JobName { get; set; }
        public string? JobNumberAndJobName { get; set; }
    }
}