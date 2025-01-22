using System.ComponentModel.DataAnnotations;

namespace FT_TTMS_WebApplication.Models
{
    // Model for passwords
    public class Passwords
    {
        [Key]
        public int Id { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
    }
}