using System.ComponentModel.DataAnnotations;

namespace FT_TTMS_WebApplication.Models
{
    // Model for duties
    public class Duty
    {
        [Key]
        public int Id { get; set; }
        public string DutyDescription { get; set; }
    }
}